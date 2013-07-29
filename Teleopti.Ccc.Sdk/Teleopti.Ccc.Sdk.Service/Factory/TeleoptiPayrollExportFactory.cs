using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class TeleoptiPayrollExportFactory
    {
        private readonly IScheduleRepository _scheduleRepository;
    	private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IAssembler<IScheduleDay, SchedulePartDto> _scheduleDayAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly ICurrentScenario _scenarioRepository;

        public TeleoptiPayrollExportFactory(IScheduleRepository scheduleRepository, ISaveSchedulePartService saveSchedulePartService,
			  ICurrentUnitOfWorkFactory unitOfWorkFactory, IAssembler<IPerson, PersonDto> personAssembler, IAssembler<IScheduleDay, 
			  SchedulePartDto> scheduleDayAssembler, IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler,
			  ICurrentScenario scenarioRepository)
        {
            _scheduleRepository = scheduleRepository;
        	_saveSchedulePartService = saveSchedulePartService;
        	_unitOfWorkFactory = unitOfWorkFactory;
        	_personAssembler = personAssembler;
            _scheduleDayAssembler = scheduleDayAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _scenarioRepository = scenarioRepository;
        }

         public ICollection<PayrollTimeExportDataDto> GetTeleoptiTimeExportData(IEnumerable<PersonDto> personCollection,
                                                                       DateOnlyDto startDate,
                                                                       DateOnlyDto endDate,
                                                                       string timeZoneInfoId,
                                                                       string specialProjection)
         {

             IList<SchedulePartDto> returnList = new List<SchedulePartDto>();
             IList<PayrollTimeExportDataDto> payrollTimeExportDataList = new List<PayrollTimeExportDataDto>();
             var timeZone = (TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId));
             var datePeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
             DateTimePeriod period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1)).ToDateTimePeriod(timeZone);

             ((DateTimePeriodAssembler)_dateTimePeriodAssembler).TimeZone = timeZone;
             using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
             {
                 IList<IPerson> personList = _personAssembler.DtosToDomainEntities(personCollection).ToList();

                 IScheduleDictionary scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(personList), new ScheduleDictionaryLoadOptions(true, false), period, _scenarioRepository.Current());
                 

                 foreach (IPerson person in personList)
                 {
                     IScheduleRange scheduleRange = scheduleDictionary[person];
                     foreach (DateOnly dateOnly in datePeriod.DayCollection())
                     {

                         IScheduleDay part = scheduleRange.ScheduledDay(dateOnly);
                         var projection = new ProjectionProvider();
                         var payrollTimeExportDto = new PayrollTimeExportDataDto();

                         payrollTimeExportDto.EmploymentNumber = person.EmploymentNumber;
                         payrollTimeExportDto.FirstName = person.Name.FirstName;
                         payrollTimeExportDto.LastName = person.Name.LastName;
                         payrollTimeExportDto.BusinessUnitName = person.Period(dateOnly).Team.BusinessUnitExplicit.Name;
                         payrollTimeExportDto.TeamName = person.Period(dateOnly).Team.Description.Name;
                         payrollTimeExportDto.SiteName = person.Period(dateOnly).Team.Site.Description.Name;
                         payrollTimeExportDto.ContractName = person.Period(dateOnly).PersonContract.Contract.Description.Name;
                         payrollTimeExportDto.PartTimePercentageName = person.Period(dateOnly).PersonContract.PartTimePercentage.Description.Name;
                         payrollTimeExportDto.Date = DateTime.Now.Date.ToShortDateString();
                         payrollTimeExportDto.StartDate = datePeriod.StartDate.ToShortDateString();
                         payrollTimeExportDto.EndDate = datePeriod.EndDate.ToShortDateString();
                         payrollTimeExportDto.ShiftCategoryName = part.PersonAssignment()!=null ? part.PersonAssignment().ShiftCategory.Description.Name : string.Empty;
                         payrollTimeExportDto.ContractTime = projection.Projection(part).ContractTime().ToString();
                         payrollTimeExportDto.WorkTime = person.Period(dateOnly).PersonContract.Contract.WorkTime.ToString();
                         payrollTimeExportDto.PaidTime = projection.Projection(part).PaidTime().ToString();
                         payrollTimeExportDto.AbsencePayrollCode = part.PersonAbsenceCollection().Count>0 ? part.PersonAbsenceCollection()[0].Layer.Payload.PayrollCode : string.Empty;
                         payrollTimeExportDto.DayOffPayrollCode = part.PersonDayOffCollection().Count>0 ? part.PersonDayOffCollection()[0].DayOff.PayrollCode : string.Empty;
                         payrollTimeExportDataList.Add(payrollTimeExportDto);
                     }
                 }
             }

             return payrollTimeExportDataList;
         }

         public ICollection<SchedulePartDto> GetTeleoptiDetailedExportData(IEnumerable<PersonDto> personCollection,
                                                                       DateOnlyDto startDate,
                                                                       DateOnlyDto endDate,
                                                                       string timeZoneInfoId,
                                                                       string specialProjection)
         {
             return new List<SchedulePartDto>();
         }

         public ICollection<SchedulePartDto> GetTeleoptiPayrollActivitiesExportData(IEnumerable<PersonDto> personCollection,
                                                                      DateOnlyDto startDate,
                                                                      DateOnlyDto endDate,
                                                                      string timeZoneInfoId,
                                                                      string specialProjection)
         {
             return new List<SchedulePartDto>();
         }
    }
}
