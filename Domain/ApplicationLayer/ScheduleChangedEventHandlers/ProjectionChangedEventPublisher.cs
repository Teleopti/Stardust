using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	[EnabledBy(Toggles.RTA_ScheduleProjectionReadOnlyHangfire_35703)]
	public class ProjectionChangedEventPublisher : 
		ProjectionChangedEventPublisherBase,
		IHandleEvent<ScheduleChangedEvent>,
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleProjection>,
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleDay>,
		IHandleEvent<ScheduleInitializeTriggeredEventForPersonScheduleDay>,
		IRunOnHangfire
	{
		public ProjectionChangedEventPublisher(IEventPublisher publisher, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IScheduleStorage scheduleStorage, IProjectionChangedEventBuilder projectionChangedEventBuilder, IProjectionVersionPersister projectionVersionPersister) : base(publisher, scenarioRepository, personRepository, scheduleStorage, projectionChangedEventBuilder, projectionVersionPersister)
		{
		}

		[AsSystem]
		[UnitOfWork]
		public override void Handle(ScheduleChangedEvent @event)
		{
			base.Handle(@event);
		}

		[AsSystem]
		[UnitOfWork]
		public override void Handle(ScheduleInitializeTriggeredEventForPersonScheduleDay @event)
		{
			base.Handle(@event);
		}

		[AsSystem]
		[UnitOfWork]
		public override void Handle(ScheduleInitializeTriggeredEventForScheduleDay @event)
		{
			base.Handle(@event);
		}

		[AsSystem]
		[UnitOfWork]
		public override void Handle(ScheduleInitializeTriggeredEventForScheduleProjection @event)
		{
			base.Handle(@event);
		}

	}

	[DisabledBy(Toggles.RTA_ScheduleProjectionReadOnlyHangfire_35703)]
	public class ProjectionChangedEventPublisherBus : 
		ProjectionChangedEventPublisherBase,
		IHandleEvent<ScheduleChangedEvent>,
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleProjection>,
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleDay>,
		IHandleEvent<ScheduleInitializeTriggeredEventForPersonScheduleDay>,
#pragma warning disable 618
		IRunOnServiceBus
#pragma warning restore 618
	{
		public ProjectionChangedEventPublisherBus(IEventPublisher publisher, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IScheduleStorage scheduleStorage, IProjectionChangedEventBuilder projectionChangedEventBuilder, IProjectionVersionPersister projectionVersionPersister) : base(publisher, scenarioRepository, personRepository, scheduleStorage, projectionChangedEventBuilder, projectionVersionPersister)
		{
		}
	}

	public class ProjectionChangedEventPublisherBase
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ProjectionChangedEventPublisher));

		private readonly IEventPublisher _publisher;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IProjectionChangedEventBuilder _projectionChangedEventBuilder;
		private readonly IProjectionVersionPersister _projectionVersionPersister;

		public ProjectionChangedEventPublisherBase(
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

		public virtual void Handle(ScheduleChangedEvent @event)
		{
			publishEvent<ProjectionChangedEvent>(@event);
		}

		public virtual void Handle(ScheduleInitializeTriggeredEventForPersonScheduleDay @event)
		{
			publishEvent<ProjectionChangedEventForPersonScheduleDay>(@event);
		}

		public virtual void Handle(ScheduleInitializeTriggeredEventForScheduleDay @event)
		{
			publishEvent<ProjectionChangedEventForScheduleDay>(@event);
		}

		public virtual void Handle(ScheduleInitializeTriggeredEventForScheduleProjection @event)
		{
			publishEvent<ProjectionChangedEventForScheduleProjection>(@event);
		}

		private void publishEvent<T>(ScheduleChangedEventBase @event) where T : ProjectionChangedEventBase, new()
		{
			var data = getData(@event);
			if (data == null) return;
			_projectionChangedEventBuilder.Build<T>(@event, data.ScheduleRange, data.RealPeriod, data.Versions)
				.ForEach(e =>
				{
					e.ScheduleLoadTimestamp = data.ScheduleLoadedTime;
					_publisher.Publish(e);
				});
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
				Logger.InfoFormat("Scenario not found (Id: {0})", @event.ScenarioId);
				return null;
			}

			var period = new DateTimePeriod(@event.StartDateTime.Subtract(TimeSpan.FromDays(1)), @event.EndDateTime);
			Logger.DebugFormat("Period start: {0}, end: {1}", period.StartDateTime, period.EndDateTime);

			var person = _personRepository.FindPeople(new[] { @event.PersonId }).FirstOrDefault();
			if (person == null)
			{
				Logger.InfoFormat("Person not found (Id: {0})", @event.PersonId);
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