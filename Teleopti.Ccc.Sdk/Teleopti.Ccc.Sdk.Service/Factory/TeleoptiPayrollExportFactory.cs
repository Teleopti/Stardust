using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
	public class TeleoptiPayrollExportFactory
	{
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IAssembler<IPerson, PersonDto> _personAssembler;
		private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISchedulePartAssembler _schedulePartAssembler;

		public TeleoptiPayrollExportFactory(IScheduleRepository scheduleRepository,
		                                    ICurrentUnitOfWorkFactory unitOfWorkFactory,
		                                    IAssembler<IPerson, PersonDto> personAssembler,
		                                    IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler,
		                                    ICurrentScenario scenarioRepository,
		                                    ISchedulePartAssembler schedulePartAssembler)
		{
			_scheduleRepository = scheduleRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_personAssembler = personAssembler;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scenarioRepository = scenarioRepository;
			_schedulePartAssembler = schedulePartAssembler;
		}

		public ICollection<PayrollBaseExportDto> GetTeleoptiTimeExportData(IEnumerable<PersonDto> personCollection,
		                                                                   DateOnlyDto startDate,
		                                                                   DateOnlyDto endDate,
		                                                                   string timeZoneInfoId,
		                                                                   string specialProjection)
		{
			var exportBuilder = new TeleoptiPayrollExportBuilder(_schedulePartAssembler, null, null, null);
			var timeExportDtos = createPayrollExportDtos(personCollection, startDate, endDate, timeZoneInfoId,
			                                             exportBuilder.BuildTimeExport);
			return timeExportDtos.ToList();
		}

		public ICollection<PayrollBaseExportDto> GetTeleoptiDetailedExportData(IEnumerable<PersonDto> personCollection,
		                                                                       DateOnlyDto startDate,
		                                                                       DateOnlyDto endDate,
		                                                                       string timeZoneInfoId,
		                                                                       string specialProjection,
		                                                                       Dictionary<Guid, AbsenceDto>
			                                                                       absenceDictinary)
		{
			var exportBuilder = new TeleoptiPayrollExportBuilder(_schedulePartAssembler, absenceDictinary, null, null);
			var detailedExportDtos = createPayrollExportDtos(personCollection, startDate, endDate, timeZoneInfoId,
			                                                 exportBuilder.BuildDetailedExport);
			return detailedExportDtos.ToList();
		}

		public ICollection<PayrollBaseExportDto> GetTeleoptiPayrollActivitiesExportData(
			IEnumerable<PersonDto> personCollection,
			DateOnlyDto startDate,
			DateOnlyDto endDate,
			string timeZoneInfoId,
			string specialProjection,
			IDictionary<Guid, AbsenceDto> absenceDictinary,
			IList<Guid> dayOffCodes,
			IDictionary<Guid, ActivityDto> activityDictionary)
		{
			var exportBuilder = new TeleoptiPayrollExportBuilder(_schedulePartAssembler, absenceDictinary, activityDictionary,
			                                                     dayOffCodes);
			var activitiesExportDtos = createPayrollExportDtos(personCollection, startDate, endDate, timeZoneInfoId,
			                                                   exportBuilder.BuildActivitesExport);
			return activitiesExportDtos.ToList();
		}

		private IEnumerable<PayrollBaseExportDto> createPayrollExportDtos(IEnumerable<PersonDto> personCollection,
		                                                                  DateOnlyDto startDate,
		                                                                  DateOnlyDto endDate,
		                                                                  string timeZoneInfoId,
		                                                                  Func
			                                                                  <IPerson, DateOnly, IScheduleDay,
			                                                                  IList<PayrollBaseExportDto>> buildExportFunction)
		{
			var payrollTimeExportDataList = new List<PayrollBaseExportDto>();
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId));
			var datePeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
			var period =
				new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1)).ToDateTimePeriod(timeZone);
			((DateTimePeriodAssembler) _dateTimePeriodAssembler).TimeZone = timeZone;

			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var personList = _personAssembler.DtosToDomainEntities(personCollection).ToList();
				var scheduleDictionary =
					_scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(personList),
					                                                   new ScheduleDictionaryLoadOptions(true, false), period,
					                                                   _scenarioRepository.Current());
				foreach (var person in personList)
				{
					var scheduleRange = scheduleDictionary[person];
					foreach (var dateOnly in datePeriod.DayCollection())
					{
						var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
						var timeExportDtos = buildExportFunction(person, dateOnly, scheduleDay);
						if (timeExportDtos.Any())
							payrollTimeExportDataList.AddRange(timeExportDtos);
					}
				}
			}
			return payrollTimeExportDataList;
		}
	}
}
