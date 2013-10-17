﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.WcfService.Factory.ScheduleReporting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class ScheduleMailFactory
    {
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly ICurrentScenario _scenarioRepository;

		  public ScheduleMailFactory(IAssembler<IPerson, PersonDto> personAssembler, ICurrentScenario scenarioRepository)
        {
            _personAssembler = personAssembler;
            _scenarioRepository = scenarioRepository;
        }

        public void SendScheduleMail(IList<PersonDto> personCollection, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneInfoId)
        {
            IRepositoryFactory repositoryFactory = new RepositoryFactory();
            IList<PersonWithScheduleStream> returnList;

            var timeZone = (TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId));
            DateOnlyPeriod originalDatePeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
            DateOnlyPeriod datePeriod = new DateOnlyPeriod(originalDatePeriod.StartDate.AddDays(-6), originalDatePeriod.EndDate.AddDays(6));
            var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1));

            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IList<IPerson> personList = _personAssembler.DtosToDomainEntities(personCollection).ToList();

                IScheduleRepository scheduleRep = repositoryFactory.CreateScheduleRepository(unitOfWork);
                IScheduleDictionary scheduleDictionary = scheduleRep.FindSchedulesOnlyForGivenPeriodAndPersons(personList, new ScheduleDictionaryLoadOptions(false, true), period, _scenarioRepository.Current());
                //rk don't know if I break stuff here...
                //scheduleDictionary.SetTimeZone(timeZone);

                ScheduleToPdfManager pdfManager = new ScheduleToPdfManager();
                returnList = pdfManager.ExportIndividual(timeZone, personList, originalDatePeriod, scheduleDictionary, ScheduleReportDetail.All);
            }

            SendEmailWithAttachedSchedule(originalDatePeriod, returnList);
        }

        private void SendEmailWithAttachedSchedule(DateOnlyPeriod datePeriod, IEnumerable<PersonWithScheduleStream> returnList)
        {
            SmtpClient smtpClient = new SmtpClient();
            var senderAddress = ((IUnsafePerson)TeleoptiPrincipal.Current).Person.Email;
            foreach (PersonWithScheduleStream personWithScheduleStream in returnList)
            {
                if (string.IsNullOrEmpty(personWithScheduleStream.Person.Email)) continue;
                CultureInfo culture = personWithScheduleStream.Person.PermissionInformation.UICulture();
                MailMessage mailMessage = new MailMessage(senderAddress, personWithScheduleStream.Person.Email,
                                                          string.Format(culture,UserTexts.Resources.ResourceManager.GetString("YourScheduleForDateParameters",culture),
                                                                        datePeriod.StartDate.Date.ToString(culture.DateTimeFormat.ShortDatePattern,culture),
                                                                        datePeriod.EndDate.Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture)),
                                                          string.Format(culture,
                                                                        UserTexts.Resources.ResourceManager.GetString("TheAttachedFileContainsScheduleForDateParameters",culture),
                                                                        datePeriod.StartDate.Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture),
                                                                        datePeriod.EndDate.Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture)));
                personWithScheduleStream.SchedulePdf.Position = 0;
                mailMessage.Attachments.Add(new Attachment(personWithScheduleStream.SchedulePdf,"schedule.pdf"));
                smtpClient.Send(mailMessage);
            }
        }
    }
}
