using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Availability
{
	public class HandleOneAnalyticsAvailabilityDay
	{
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IAnalyticsHourlyAvailabilityRepository _analyticsHourlyAvailabilityRepository;
		private readonly FetchAnalyticsScenarios _fetchAnalyticsScenarios;
		private static readonly ILog logger = LogManager.GetLogger(typeof(HandleOneAnalyticsAvailabilityDay));

		public HandleOneAnalyticsAvailabilityDay(IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository, IAnalyticsDateRepository analyticsDateRepository,
			IAnalyticsHourlyAvailabilityRepository analyticsHourlyAvailabilityRepository,
			FetchAnalyticsScenarios fetchAnalyticsScenarios)
		{
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsDateRepository = analyticsDateRepository;
			_analyticsHourlyAvailabilityRepository = analyticsHourlyAvailabilityRepository;
			_fetchAnalyticsScenarios = fetchAnalyticsScenarios;
		}

		[AnalyticsUnitOfWork]
		public virtual void Execute(IPerson person, DateOnly date, Dictionary<IScenario, IScheduleDictionary> schedules,
			IEnumerable<IStudentAvailabilityDay> availabilityDays)
		{
			var analyticsDate = getAnalyticsDate(date);
			var scenarios = _fetchAnalyticsScenarios.Execute().ToDictionary(s => s.ScenarioCode.Value);

			if (!availabilityDays.Any())
			{
				foreach (var scenario in scenarios)
				{
					logger.Debug($"Deleting availability for Date:{analyticsDate.DateId}, Scenario:{scenario.Key}");
					_analyticsHourlyAvailabilityRepository.Delete(person.Id.Value, analyticsDate.DateId, scenario.Value.ScenarioId);
				}
				return;
			}

			var analyticsPersonPeriod = getAnalyticsPersonPeriod(date, person);
			if (analyticsPersonPeriod == null)
			{
				logger.Debug($"No person period found in application for person {person.Id.Value} on date {date}");
				return;
			}

			// There should be only one record, but may exists multiple (Refer to bug #76978).
			var availabilityDay = availabilityDays.OrderByDescending(x=>x.UpdatedOn).First();
			foreach (var schedule in schedules)
			{
				var scheduledDay = schedule.Value[person].ScheduledDay(date);
				var scheduledTime = scheduledWorkTime(scheduledDay);

				var analyticsScenario = scenarios[schedule.Key.Id.Value];
				var analyticsHourlyAvailability = new AnalyticsHourlyAvailability
				{
					AvailableDays = 1,
					AvailableTimeMinutes = getMaxAvailable(availabilityDay),
					BusinessUnitId = analyticsScenario.BusinessUnitId,
					DateId = analyticsDate.DateId,
					PersonId = analyticsPersonPeriod.PersonId,
					ScenarioId = analyticsScenario.ScenarioId,
					ScheduledDays = Convert.ToInt32(scheduledTime > 0),
					ScheduledTimeMinutes = scheduledTime
				};
				if (logger.IsDebugEnabled)
				{
					logger.Debug($"Adding or updating availability for PersonPeriod:{analyticsPersonPeriod.PersonId}, Date:{analyticsDate.DateId}, Scenario:{analyticsScenario.ScenarioId}");
				}
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