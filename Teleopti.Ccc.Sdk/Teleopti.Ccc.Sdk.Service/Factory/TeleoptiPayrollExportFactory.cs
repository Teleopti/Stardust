using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.Contracts;
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
        private readonly ISchedulePartAssembler _schedulePartAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly ICurrentScenario _scenarioRepository;
	    private Dictionary<Guid, AbsenceDto> _absenceDictinary;

	    public TeleoptiPayrollExportFactory(IScheduleRepository scheduleRepository,
	                                        ISaveSchedulePartService saveSchedulePartService,
	                                        ICurrentUnitOfWorkFactory unitOfWorkFactory,
	                                        IAssembler<IPerson, PersonDto> personAssembler,
	                                        ISchedulePartAssembler schedulePartAssembler,
	                                        IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler,
	                                        ICurrentScenario scenarioRepository)
        {
            _scheduleRepository = scheduleRepository;
        	_saveSchedulePartService = saveSchedulePartService;
        	_unitOfWorkFactory = unitOfWorkFactory;
        	_personAssembler = personAssembler;
            _schedulePartAssembler = schedulePartAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _scenarioRepository = scenarioRepository;
        }

         public ICollection<PayrollTimeExportDataDto> GetTeleoptiTimeExportData(IEnumerable<PersonDto> personCollection,
                                                                       DateOnlyDto startDate,
                                                                       DateOnlyDto endDate,
                                                                       string timeZoneInfoId,
                                                                       string specialProjection)
         {
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

	    public ICollection<PayrollDetailedExportDto> GetTeleoptiDetailedExportData(IEnumerable<PersonDto> personCollection,
	                                                                               DateOnlyDto startDate,
	                                                                               DateOnlyDto endDate,
	                                                                               string timeZoneInfoId,
	                                                                               string specialProjection,
	                                                                               ITeleoptiSchedulingService
		                                                                               schedulingService)
	    {
		    var payrollDetailedExportDataList = new List<PayrollDetailedExportDto>();
		    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId);
		    var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
		    var period =
			    new DateOnlyPeriod(dateOnlyPeriod.StartDate, dateOnlyPeriod.EndDate.AddDays(1)).ToDateTimePeriod(timeZone);
		    ((DateTimePeriodAssembler) _dateTimePeriodAssembler).TimeZone = timeZone;

		    prepareAbsenceDictionary(schedulingService);

		    using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
		    {
			    var personList = _personAssembler.DtosToDomainEntities(personCollection).ToList();
			    var scheduleDictionary =
				    _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(personList),
				                                                       new ScheduleDictionaryLoadOptions(
					                                                       true, false), period,
				                                                       _scenarioRepository.Current());
			    foreach (var person in personList)
			    {
				    var scheduleRange = scheduleDictionary[person];
				    foreach (var dateOnly in dateOnlyPeriod.DayCollection())
				    {
					    var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
					    
						var overtimeAndShiftAllowanceDtos = getOvertimeAndShiftAllowances(dateOnly, person, scheduleDay).ToList();
						if (overtimeAndShiftAllowanceDtos.Any())
							payrollDetailedExportDataList.AddRange(overtimeAndShiftAllowanceDtos);

					    var schedulePartDto = _schedulePartAssembler.DomainEntityToDto(scheduleDay);
						var absences = getAbsences(dateOnly, person, schedulePartDto).ToList();
						if (absences.Any())
							payrollDetailedExportDataList.AddRange(absences);
				    }
			    }
		    }

		    return payrollDetailedExportDataList;
	    }

		private static IEnumerable<PayrollDetailedExportDto> getOvertimeAndShiftAllowances(DateOnly dateOnly, IPerson person, IScheduleDay scheduleDay)
		{
			var multiplicatorProjectionService = new MultiplicatorProjectionService(scheduleDay, dateOnly);
			var projection = multiplicatorProjectionService.CreateProjection();
			foreach (var layer in projection)
			{
				var payrollDetailedExportDto = initializeDtoWithPersonData(person, dateOnly);
				payrollDetailedExportDto.PayrollCode = layer.Payload.ExportCode;
				payrollDetailedExportDto.Time = layer.Period.ElapsedTime();
				yield return payrollDetailedExportDto;
			}
		}

		private IEnumerable<PayrollDetailedExportDto> getAbsences(DateOnly dateOnly, IPerson person, SchedulePartDto schedulePartDto)
	    {
		    var absencesInProjection = schedulePartDto.ProjectedLayerCollection
		                                              .Where(layer => layer.IsAbsence &&
		                                                              layer.ContractTime != TimeSpan.Zero);
		    foreach (var layer in absencesInProjection)
			{
				var payrollDetailedExportDto = initializeDtoWithPersonData(person, dateOnly);
			    payrollDetailedExportDto.PayrollCode = _absenceDictinary[layer.PayloadId].PayrollCode;
			    payrollDetailedExportDto.Time = layer.ContractTime;
			    yield return payrollDetailedExportDto;
		    }
	    }

		private static PayrollDetailedExportDto initializeDtoWithPersonData(IPerson person, DateOnly dateOnly)
		{
			var payrollDetailedExportDto = new PayrollDetailedExportDto
				{
					EmploymentNumber = person.EmploymentNumber,
					FirstName = person.Name.FirstName,
					LastName = person.Name.LastName,
					BusinessUnitName = person.Period(dateOnly).Team.BusinessUnitExplicit.Name,
					SiteName = person.Period(dateOnly).Team.Site.Description.Name,
					TeamName = person.Period(dateOnly).Team.Description.Name,
					ContractName = person.Period(dateOnly).PersonContract.Contract.Description.Name,
					PartTimePercentageName = person.Period(dateOnly).PersonContract.PartTimePercentage.Description.Name,
					PartTimePercentageNumber = person.Period(dateOnly).PersonContract.PartTimePercentage.Percentage.Value,
					Date = DateTime.Now.Date.ToShortDateString()
				};
			return payrollDetailedExportDto;
		}

	    private void prepareAbsenceDictionary(ITeleoptiSchedulingService schedulingService)
	    {
		    var absences = schedulingService.GetAbsences(new AbsenceLoadOptionDto {LoadDeleted = true});
		    _absenceDictinary = new Dictionary<Guid, AbsenceDto>();

		    foreach (var absence in absences)
				if (absence.Id.HasValue)
			    _absenceDictinary.Add(absence.Id.Value, absence);
			
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
