﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Availability
{
	public class AnalyticsAvailabilityUpdater : 
		IHandleEvent<AvailabilityChangedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsAvailabilityUpdater));
		private readonly IStudentAvailabilityDayRepository _availabilityDayRepository;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IAnalyticsScenarioRepository _analyticsScenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IAnalyticsHourlyAvailabilityRepository _analyticsHourlyAvailabilityRepository;
		private readonly IScenarioRepository _scenarioRepository;

		public AnalyticsAvailabilityUpdater(IStudentAvailabilityDayRepository availabilityDayRepository, IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository, IAnalyticsDateRepository analyticsDateRepository, IAnalyticsScenarioRepository analyticsScenarioRepository, IPersonRepository personRepository, IScheduleStorage scheduleStorage, IAnalyticsHourlyAvailabilityRepository analyticsHourlyAvailabilityRepository, IScenarioRepository scenarioRepository)
		{
			_availabilityDayRepository = availabilityDayRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsDateRepository = analyticsDateRepository;
			_analyticsScenarioRepository = analyticsScenarioRepository;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_analyticsHourlyAvailabilityRepository = analyticsHourlyAvailabilityRepository;
			_scenarioRepository = scenarioRepository;

			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of handler was created");
			}
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(AvailabilityChangedEvent @event)
		{
			var person = _personRepository.Get(@event.PersonId);
			if (person == null)
			{
				logger.Debug($"No person found for personId {@event.PersonId}");
				return;
			}
			var analyticsPersonPeriod = getAnalyticsPersonPeriod(@event, person);
			var availabilityDays = _availabilityDayRepository.Find(@event.Date, person);
			var date = getAnalyticsDate(@event);
			var scenarios = getScenarios();

			if (!availabilityDays.Any())
			{
				foreach (var scenario in scenarios)
				{
					logger.Debug($"Deleting availability for PersonPeriod:{analyticsPersonPeriod.PersonId}, Date:{date.DateId}, Scenario:{scenario.ScenarioId}");
					_analyticsHourlyAvailabilityRepository.Delete(analyticsPersonPeriod.PersonId, date.DateId, scenario.ScenarioId);
				}
				return;
			}
			var availabilityDay = availabilityDays.Single(); // There can be only one!
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
					DateId = date.DateId,
					PersonId = analyticsPersonPeriod.PersonId,
					ScenarioId = scenario.ScenarioId,
					ScheduledDays = Convert.ToInt32(scheduledTime > 0),
					ScheduledTimeMinutes = scheduledTime
				};
				logger.Debug($"Adding or updating availability for PersonPeriod:{analyticsPersonPeriod.PersonId}, Date:{date.DateId}, Scenario:{scenario.ScenarioId}");
				_analyticsHourlyAvailabilityRepository.AddOrUpdate(analyticsHourlyAvailability);
			}
		}

		private AnalyticsPersonPeriod getAnalyticsPersonPeriod(AvailabilityChangedEvent @event, IPerson person)
		{
			var personPeriod = person.Period(@event.Date);
			if (personPeriod?.Id == null)
				throw new ApplicationException("Person period was not found for person in application");
			var analyticsPersonPeriod = _analyticsPersonPeriodRepository.PersonPeriod(personPeriod.Id.GetValueOrDefault());
			if (analyticsPersonPeriod == null)
				throw new PersonPeriodMissingInAnalyticsException(personPeriod.Id.GetValueOrDefault());
			return analyticsPersonPeriod;
		}

		private IAnalyticsDate getAnalyticsDate(AvailabilityChangedEvent @event)
		{
			var date = _analyticsDateRepository.Date(@event.Date.Date);
			if (date == null)
				throw new DateMissingInAnalyticsException(@event.Date.Date);
			return date;
		}

		private IList<AnalyticsScenario> getScenarios()
		{
			var scenarios = _analyticsScenarioRepository.Scenarios();
			return scenarios.Where(x => !x.IsDeleted && x.ScenarioId != -1).ToList();
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
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false, false) { LoadDaysAfterLeft = true };
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
			if (scheduleDay.IsScheduled())
			{
				var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();

				if (visualLayerCollection.HasLayers)
				{
					minutes = (int)visualLayerCollection.WorkTime().TotalMinutes;
				}
			}
			return minutes;
		}
	}
}