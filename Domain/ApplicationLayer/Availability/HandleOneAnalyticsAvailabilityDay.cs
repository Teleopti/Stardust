using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Availability
{
	public class HandleOneAnalyticsAvailabilityDay
	{
		private readonly IStudentAvailabilityDayRepository _availabilityDayRepository;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IAnalyticsHourlyAvailabilityRepository _analyticsHourlyAvailabilityRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly FetchAnalyticsScenarios _fetchAnalyticsScenarios;
		private static readonly ILog logger = LogManager.GetLogger(typeof(HandleOneAnalyticsAvailabilityDay));

		public HandleOneAnalyticsAvailabilityDay(IStudentAvailabilityDayRepository availabilityDayRepository,
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository, IAnalyticsDateRepository analyticsDateRepository,
			IPersonRepository personRepository,
			IScheduleStorage scheduleStorage, IAnalyticsHourlyAvailabilityRepository analyticsHourlyAvailabilityRepository,
			IScenarioRepository scenarioRepository, FetchAnalyticsScenarios fetchAnalyticsScenarios)
		{
			_availabilityDayRepository = availabilityDayRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsDateRepository = analyticsDateRepository;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_analyticsHourlyAvailabilityRepository = analyticsHourlyAvailabilityRepository;
			_scenarioRepository = scenarioRepository;
			_fetchAnalyticsScenarios = fetchAnalyticsScenarios;
		}

		[AnalyticsUnitOfWork]
		public virtual void Execute(Guid personId, DateOnly date)
		{
			var person = _personRepository.Get(personId);
			if (person == null)
			{
				logger.Debug($"No person found for personId {personId}");
				return;
			}
			var availabilityDays = _availabilityDayRepository.Find(date, person);
			var analyticsDate = getAnalyticsDate(date);
			var scenarios = _fetchAnalyticsScenarios.Execute();

			if (!availabilityDays.Any())
			{
				foreach (var scenario in scenarios)
				{
					logger.Debug($"Deleting availability for Date:{analyticsDate.DateId}, Scenario:{scenario.ScenarioId}");
					_analyticsHourlyAvailabilityRepository.Delete(personId, analyticsDate.DateId, scenario.ScenarioId);
				}
				return;
			}

			var analyticsPersonPeriod = getAnalyticsPersonPeriod(date, person);
			if (analyticsPersonPeriod == null)
			{
				logger.Debug($"No person period found in application for person {personId} on date {date}");
				return;
			}

			// There should be only one record, but may exists multiple (Refer to bug #76978).
			var availabilityDay = availabilityDays.OrderByDescending(x=>x.UpdatedOn).First();
			foreach (var scenario in scenarios)
			{
				var scheduledDay = getScheduledDay(availabilityDay, scenario.ScenarioCode.GetValueOrDefault());
				if (scheduledDay == null)
				{
					logger.Debug($"No schedule day found for scenario {scenario.ScenarioCode.GetValueOrDefault()} and day {availabilityDay.RestrictionDate}");
					continue;
				}
				var scheduledTime = scheduledWorkTime(scheduledDay);

				var analyticsHourlyAvailability = new AnalyticsHourlyAvailability
				{
					AvailableDays = 1,
					AvailableTimeMinutes = getMaxAvailable(availabilityDay),
					BusinessUnitId = scenario.BusinessUnitId,
					DateId = analyticsDate.DateId,
					PersonId = analyticsPersonPeriod.PersonId,
					ScenarioId = scenario.ScenarioId,
					ScheduledDays = Convert.ToInt32(scheduledTime > 0),
					ScheduledTimeMinutes = scheduledTime
				};
				logger.Debug($"Adding or updating availability for PersonPeriod:{analyticsPersonPeriod.PersonId}, Date:{analyticsDate.DateId}, Scenario:{scenario.ScenarioId}");
				_analyticsHourlyAvailabilityRepository.AddOrUpdate(analyticsHourlyAvailability);
			}
		}

		private AnalyticsPersonPeriod getAnalyticsPersonPeriod(DateOnly date, IPerson person)
		{
			var personPeriod = person.Period(date);
			if (personPeriod?.Id == null)
				return null;
			var analyticsPersonPeriod = _analyticsPersonPeriodRepository.PersonPeriod(personPeriod.Id.GetValueOrDefault());
			if (analyticsPersonPeriod == null)
				throw new PersonPeriodMissingInAnalyticsException(personPeriod.Id.GetValueOrDefault());
			return analyticsPersonPeriod;
		}

		private IAnalyticsDate getAnalyticsDate(DateOnly date)
		{
			var analyticsDate = _analyticsDateRepository.Date(date.Date);
			if (analyticsDate == null)
				throw new DateMissingInAnalyticsException(date.Date);
			return analyticsDate;
		}

		private IScheduleDay getScheduledDay(IStudentAvailabilityDay availabilityDay, Guid scenarioId)
		{
			var scenario = _scenarioRepository.Get(scenarioId);
			if (scenario == null)
			{
				logger.Warn($"Scenario {scenarioId} does not exist in Application database, skipping.");
				return null;
			}
			if (!scenario.EnableReporting)
			{
				logger.Debug($"Scenario {scenarioId} is not reportable, skipping.");
				return null;
			}
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false, false)
			{
				LoadDaysAfterLeft = true
			};
			var day = availabilityDay.RestrictionDate;
			var period = day.ToDateOnlyPeriod();
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(availabilityDay.Person,
				scheduleDictionaryLoadOptions,
				period, scenario);
			var scheduledDay = scheduleDictionary[availabilityDay.Person].ScheduledDay(day);
			return scheduledDay;
		}

		private static int getMaxAvailable(IStudentAvailabilityDay availabilityDay)
		{
			var availRestriction = availabilityDay.RestrictionCollection.FirstOrDefault();
			if (availRestriction == null)
				throw new ArgumentException("Restriction missing from Availability");

			var start = TimeSpan.FromMinutes(0);
			var end = TimeSpan.FromHours(24);
			if (availRestriction.StartTimeLimitation.StartTime.HasValue)
				start = availRestriction.StartTimeLimitation.StartTime.GetValueOrDefault();

			if (availRestriction.EndTimeLimitation.EndTime.HasValue)
				end = availRestriction.EndTimeLimitation.EndTime.GetValueOrDefault();

			var minutes = (int)end.Add(-start).TotalMinutes;

			if (availRestriction.WorkTimeLimitation.EndTime.HasValue)
			{
				minutes = (int)availRestriction.WorkTimeLimitation.EndTime.Value.TotalMinutes;
			}
			return minutes;
		}

		private static int scheduledWorkTime(IScheduleDay scheduleDay)
		{
			var minutes = 0;
			if (!scheduleDay.IsScheduled()) return minutes;

			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			if (visualLayerCollection.HasLayers)
			{
				minutes = (int)visualLayerCollection.WorkTime().TotalMinutes;
			}
			return minutes;
		}
	}
}