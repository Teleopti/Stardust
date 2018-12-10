using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
	public class TeleoptiPayrollExportFactory
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISchedulePartAssembler _schedulePartAssembler;

		public TeleoptiPayrollExportFactory(IScheduleStorage scheduleStorage,
		                                    ICurrentUnitOfWorkFactory unitOfWorkFactory,
		                                    IPersonRepository personRepository,
		                                    IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler,
		                                    ICurrentScenario scenarioRepository,
		                                    ISchedulePartAssembler schedulePartAssembler)
		{
			_scheduleStorage = scheduleStorage;
			_unitOfWorkFactory = unitOfWorkFactory;
			_personRepository = personRepository;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_scenarioRepository = scenarioRepository;
			_schedulePartAssembler = schedulePartAssembler;
		}

		public ICollection<PayrollBaseExportDto> GetTeleoptiTimeExportData(IEnumerable<PersonDto> personCollection,
		                                                                   DateOnlyDto startDate,
		                                                                   DateOnlyDto endDate,
		                                                                   string timeZoneInfoId)
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
		                                                                       Dictionary<Guid, AbsenceDto> absenceDictinary)
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
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId);
			var datePeriod = new DateOnlyPeriod(startDate.ToDateOnly(), endDate.ToDateOnly());
			var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1));
			((DateTimePeriodAssembler) _dateTimePeriodAssembler).TimeZone = timeZone;

			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var personList = _personRepository.FindPeople(personCollection.Select(p => p.Id.GetValueOrDefault()).ToArray());
				var scheduleDictionary =
					_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
						personList,
						new ScheduleDictionaryLoadOptions(false, false),
						period,
						_scenarioRepository.Current()
						);

				var exportDtos = getExportDtosForPersons(personList, scheduleDictionary, datePeriod, buildExportFunction).ToArray();
				if (exportDtos.Any())
					payrollTimeExportDataList.AddRange(exportDtos);
			}
			return payrollTimeExportDataList;
		}

		private static IEnumerable<PayrollBaseExportDto> getExportDtosForPersons(IEnumerable<IPerson> personList,
		                                                                         IScheduleDictionary scheduleDictionary,
		                                                                         DateOnlyPeriod datePeriod,
		                                                                         Func
			                                                                         <IPerson, DateOnly, IScheduleDay,
			                                                                         IList<PayrollBaseExportDto>>
			                                                                         buildExportFunction)
		{
			var payrollTimeExportDataList = new List<PayrollBaseExportDto>();
			foreach (var person in personList)
			{
				var scheduleRange = scheduleDictionary[person];
				foreach (var scheduleDay in scheduleRange.ScheduledDayCollection(datePeriod))
				{
					if (afterTerminationDate(person, scheduleDay.DateOnlyAsPeriod.DateOnly) || isBeforeFirstPeriod(person, scheduleDay.DateOnlyAsPeriod.DateOnly))
						continue;

					var payrollExportDtos = buildExportFunction(person, scheduleDay.DateOnlyAsPeriod.DateOnly, scheduleDay);
					if (payrollExportDtos.Any())
						payrollTimeExportDataList.AddRange(payrollExportDtos);
				}
			}
			return payrollTimeExportDataList;
		}

		private static bool afterTerminationDate(IPerson person, DateOnly dateOnly)
		{
			var terminateDate = person.TerminalDate;
			return terminateDate.HasValue && dateOnly > terminateDate;
		}

		private static bool isBeforeFirstPeriod(IPerson person, DateOnly dateOnly)
		{
			var firstPeriod = person.PersonPeriodCollection.OrderBy(p => p.Period.StartDate).FirstOrDefault();
			return firstPeriod!=null && dateOnly < firstPeriod.Period.StartDate;
		}
	}
}
