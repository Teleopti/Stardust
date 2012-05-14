﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class ScheduleFactory
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScheduleDictionarySaver _scheduleDictionarySaver;
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IAssembler<IScheduleDay, SchedulePartDto> _scheduleDayAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly IScenarioProvider _scenarioProvider;

        public ScheduleFactory(IScheduleRepository scheduleRepository, IScheduleDictionarySaver scheduleDictionarySaver, IAssembler<IPerson, PersonDto> personAssembler, IAssembler<IScheduleDay, SchedulePartDto> scheduleDayAssembler, IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IScenarioProvider scenarioProvider)
        {
            _scheduleRepository = scheduleRepository;
            _scheduleDictionarySaver = scheduleDictionarySaver;
            _personAssembler = personAssembler;
            _scheduleDayAssembler = scheduleDayAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _scenarioProvider = scenarioProvider;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        internal ICollection<MultiplicatorDataDto> CreateMultiplicatorData(ICollection<PersonDto> personCollection, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneInfoId)
        {
            IList<MultiplicatorDataDto> multiplicatorDataDtos = new List<MultiplicatorDataDto>();

            var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId));
            var datePeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
            DateTimePeriod period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1)).ToDateTimePeriod(timeZone);
            
            using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IList<IPerson> personList = _personAssembler.DtosToDomainEntities(personCollection).ToList();
                IScheduleDictionary scheduleDictonary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(personList), new ScheduleDictionaryLoadOptions(false, false), period, _scenarioProvider.DefaultScenario());

                //rk don't know if I break stuff here...
                //scheduleDictonary.SetTimeZone(timeZone);
                foreach (IPerson person in personList)
                {
                    IScheduleRange scheduleRange = scheduleDictonary[person];
                    foreach (DateOnly dateOnly in datePeriod.DayCollection())
                    {
                        var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
                        var multiplicatorProjectionService = new MultiplicatorProjectionService(scheduleDay, dateOnly);
                        //Create Activity Layers
                        foreach (IMultiplicatorLayer layer in multiplicatorProjectionService.CreateProjection())
                        {
                            var multiplicatorDataDto = new MultiplicatorDataDto
                                                           {
                                                               Date = dateOnly,
                                                               ActualDate =
                                                                   layer.Period.StartDateTimeLocal(scheduleDay.TimeZone)
                                                                   .Date,
                                                               Amount = layer.Period.ElapsedTime(),
                                                               Multiplicator = new MultiplicatorDto(layer.Payload),
                                                               PersonId = person.Id
                                                           };

                            multiplicatorDataDtos.Add(multiplicatorDataDto);
                        }
                    }
                }
            }

            return multiplicatorDataDtos;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public ICollection<SchedulePartDto> CreateSchedulePartCollection(IEnumerable<PersonDto> personCollection, 
																		DateOnlyDto startDate, 
																		DateOnlyDto endDate, 
																		string timeZoneInfoId,
																		string specialProjection)
        {
            IList<SchedulePartDto> returnList = new List<SchedulePartDto>();

            var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId));
            var datePeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
            DateTimePeriod period = new DateOnlyPeriod(datePeriod.StartDate,datePeriod.EndDate.AddDays(1)).ToDateTimePeriod(timeZone);

            ((DateTimePeriodAssembler) _dateTimePeriodAssembler).TimeZone = timeZone;
            using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IList<IPerson> personList = _personAssembler.DtosToDomainEntities(personCollection).ToList();

                IScheduleDictionary scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(personList), new ScheduleDictionaryLoadOptions(true, false), period, _scenarioProvider.DefaultScenario());
                foreach (IPerson person in personList)
                {
                    IScheduleRange scheduleRange = scheduleDictionary[person];
                    foreach (DateOnly dateOnly in datePeriod.DayCollection())
                    {
                        IScheduleDay part = scheduleRange.ScheduledDay(dateOnly);
						//rk - ugly hack until ScheduleProjectionService is stateless (=not depended on schedulday in ctor)
						//when that's done - inject a IProjectionService instead.
                    	var schedAss = ((SchedulePartAssembler) _scheduleDayAssembler);
						schedAss.SpecialProjection = specialProjection;
                    	schedAss.TimeZone = timeZone;
                        returnList.Add(_scheduleDayAssembler.DomainEntityToDto(part));
                    }
                }
            }

            return returnList;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal void SaveSchedulePart(SchedulePartDto schedulePartDto)
        {
            using (new MessageBrokerSendEnabler())
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IScheduleDay schedulePart = _scheduleDayAssembler.DtoToDomainEntity(schedulePartDto);
                    var dic = (ReadOnlyScheduleDictionary)schedulePart.Owner;
                    dic.MakeEditable();
                    IList<IBusinessRuleResponse> skipList = new List<IBusinessRuleResponse>();
                    IBusinessRuleResponse toAdd = new BusinessRuleResponse(typeof (OpenHoursRule), "", false, false,
                                                                           new DateTimePeriod(), schedulePart.Person, new DateOnlyPeriod());
                    skipList.Add(toAdd);

                    //inga regler för nu
                    var invalidList = dic.Modify(ScheduleModifier.Scheduler,
                                                                          schedulePart,
                                                                          NewBusinessRuleCollection.Minimum(),new EmptyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));

                    if (invalidList.Count() > 0)
                        throw new NotImplementedException("FIX THIS LATER" + invalidList.First().Message);

                    _scheduleDictionarySaver.MarkForPersist(uow, _scheduleRepository, dic.DifferenceSinceSnapshot());
                    uow.PersistAll();
                }
            }
        }
    }
}
