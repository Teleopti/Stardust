using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Availability
{
	public class HandleMultipleAnalyticsAvailabilityDays
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(HandleMultipleAnalyticsAvailabilityDays));

		private readonly HandleOneAnalyticsAvailabilityDay _handleOneAnalyticsAvailabilityDay;
		private readonly IPersonRepository _personRepository;
		private readonly IStudentAvailabilityDayRepository _studentAvailabilityDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleStorage _scheduleStorage;

		public HandleMultipleAnalyticsAvailabilityDays(
			HandleOneAnalyticsAvailabilityDay handleOneAnalyticsAvailabilityDay, IPersonRepository personRepository,
			IStudentAvailabilityDayRepository studentAvailabilityDayRepository, IScenarioRepository scenarioRepository,
			IScheduleStorage scheduleStorage)
		{
			_handleOneAnalyticsAvailabilityDay = handleOneAnalyticsAvailabilityDay;
			_personRepository = personRepository;
			_studentAvailabilityDayRepository = studentAvailabilityDayRepository;
			_scenarioRepository = scenarioRepository;
			_scheduleStorage = scheduleStorage;
		}

		public void Execute(Guid personId, IEnumerable<DateOnly> dates)
		{
			var dateOnlies = dates.ToArray();
			if (!dateOnlies.Any()) return;

			var person = _personRepository.Get(personId);
			if (person == null)
			{
				logger.Debug($"No person found for personId {personId}");
				return;
			}

			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false, false)
			{
				LoadDaysAfterLeft = true
			};
			
			var period = new DateOnlyPeriod(dateOnlies.Min(), dateOnlies.Max());
			
			var reportableScenarios = _scenarioRepository.FindEnabledForReportingSorted();
			var schedules = reportableScenarios.ToDictionary(k => k,
				v => _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, scheduleDictionaryLoadOptions,
					period, v));

			var availabilityDays = _studentAvailabilityDayRepository.Find(period, new []{person}).ToLookup(k => k.RestrictionDate);
			foreach (var date in dateOnlies)
			{
				_handleOneAnalyticsAvailabilityDay.Execute(person, date, schedules, availabilityDays[date]);
			}
		}

	}
}