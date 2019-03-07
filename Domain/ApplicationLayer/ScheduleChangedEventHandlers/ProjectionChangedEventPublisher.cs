using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventPublisher : 
		IHandleEvent<ScheduleChangedEvent>,
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleProjection>,
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleDay>,
		IHandleEvent<ScheduleInitializeTriggeredEventForPersonScheduleDay>,
		IRunOnHangfire
	{

		private static readonly ILog Logger = LogManager.GetLogger(typeof(ProjectionChangedEventPublisher));

		private readonly IEventPublisher _publisher;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IProjectionChangedEventBuilder _projectionChangedEventBuilder;
		private readonly IProjectionVersionPersister _projectionVersionPersister;

		public ProjectionChangedEventPublisher(
			IEventPublisher publisher,
			IScenarioRepository scenarioRepository,
			IPersonRepository personRepository,
			IScheduleStorage scheduleStorage,
			IProjectionChangedEventBuilder projectionChangedEventBuilder,
			IProjectionVersionPersister projectionVersionPersister)
		{
			_publisher = publisher;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_projectionChangedEventBuilder = projectionChangedEventBuilder;
			_projectionVersionPersister = projectionVersionPersister;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			var projectionChangedEvents = publishEvent<ProjectionChangedEventNew>(@event);
			publishProjectChangedEventForShiftExchangeOffer(projectionChangedEvents);
		}

		//put here for perf reasons
		protected void publishProjectChangedEventForShiftExchangeOffer(IEnumerable<ProjectionChangedEventBase> projectionChangedEvents)
		{
			projectionChangedEvents.ForEach(e =>
			{
				if (e.IsDefaultScenario)
				{
					_publisher.Publish(
					new ProjectionChangedEventForShiftExchangeOffer
					{
						LogOnBusinessUnitId = e.LogOnBusinessUnitId,
						LogOnDatasource = e.LogOnDatasource,
						PersonId = e.PersonId,
						Days =  e.ScheduleDays.Select(x => new ProjectionChangedEventForShiftExchangeOfferDateAndChecksums { Date = x.Date, Checksum = x.CheckSum }).ToList()			
					});	
				}
			});
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ScheduleInitializeTriggeredEventForPersonScheduleDay @event)
		{
			publishEvent<ProjectionChangedEventForPersonScheduleDay>(@event);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ScheduleInitializeTriggeredEventForScheduleDay @event)
		{
			publishEvent<ProjectionChangedEventForScheduleDay>(@event);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ScheduleInitializeTriggeredEventForScheduleProjection @event)
		{
			publishEvent<ProjectionChangedEventForScheduleProjection>(@event);
		}


		protected IEnumerable<T> publishEvent<T>(ScheduleChangedEventBase @event) where T : ProjectionChangedEventBase, new()
		{
			var data = getData(@event);
			if (data == null) return new T[]{};
			var events = _projectionChangedEventBuilder.Build<T>(@event, data.ScheduleRange, data.RealPeriod, data.Versions);
			events.ForEach(e =>
				{

					e.ScheduleLoadTimestamp = data.ScheduleLoadedTime;
					_publisher.Publish(e);
				});
			return events;
		}

		private class range
		{
			public IScheduleRange ScheduleRange;
			public DateOnlyPeriod RealPeriod;
			public DateTime ScheduleLoadedTime;
			public IEnumerable<ProjectionVersion> Versions;
		}

		private range getData(ScheduleChangedEventBase @event)
		{
			var scenario = _scenarioRepository.Get(@event.ScenarioId);
			if (scenario == null)
			{
				Logger.Info($"Scenario not found (Id: {@event.ScenarioId})");
				return null;
			}

			var period = new DateTimePeriod(@event.StartDateTime.Subtract(TimeSpan.FromDays(1)), @event.EndDateTime);
			Logger.Debug($"Period start: {period.StartDateTime}, end: {period.EndDateTime}");

			var person = _personRepository.FindPeople(new[] { @event.PersonId }).FirstOrDefault();
			if (person == null)
			{
				Logger.Info($"Person not found (Id: {@event.PersonId})");
				return null;
			}

			// gets the date period the same way as FindSchedulesForPersonOnlyInGivenPeriod, hence duplication
			var dateOnlyPeriod = period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			var versions = _projectionVersionPersister.LockAndGetVersions(@event.PersonId, dateOnlyPeriod.StartDate, dateOnlyPeriod.EndDate);

			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				scenario);

			var range = schedule[person];

			var actualPeriod = @event.SkipDelete ? range.TotalPeriod() : period;

			if (!actualPeriod.HasValue) return null;

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var realPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);

			Logger.DebugFormat("RealPeriod start: {0}, end: {1}", realPeriod.StartDate, realPeriod.EndDate);
			return new range
			{
				ScheduleRange = range,
				RealPeriod = realPeriod,
				ScheduleLoadedTime = schedule.ScheduleLoadedTime,
				Versions = versions
			};
		}

	}
	
}