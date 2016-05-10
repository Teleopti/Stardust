using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
#pragma warning disable 618
	public class ProjectionChangedEventPublisher : 
		IHandleEvent<ScheduleChangedEvent>, 
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleProjection>,
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleDay>,
		IHandleEvent<ScheduleInitializeTriggeredEventForPersonScheduleDay>,
		IRunOnServiceBus
#pragma warning restore 618
	{
		private readonly IEventPublisher _publisher;
	    private static readonly ILog Logger = LogManager.GetLogger(typeof (ProjectionChangedEventPublisher));
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IProjectionChangedEventBuilder _projectionChangedEventBuilder;
		private readonly INow _now;
		private readonly IProjectionVersionPersister _projectionVersionPersister;

		public ProjectionChangedEventPublisher(IEventPublisher publisher, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IScheduleStorage scheduleStorage, IProjectionChangedEventBuilder projectionChangedEventBuilder, INow now, IProjectionVersionPersister projectionVersionPersister)
		{
			_publisher = publisher;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_projectionChangedEventBuilder = projectionChangedEventBuilder;
			_now = now;
			_projectionVersionPersister = projectionVersionPersister;
		}
		
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			// select projection version with update lock
			// OR insert projection version (if duplicate, it will throw and hence retry)
			publishEvent<ProjectionChangedEvent>(@event);
			// update the version
			// optional, if no update lock, will throw because the version changed by someone else
		}

		public void Handle(ScheduleInitializeTriggeredEventForPersonScheduleDay @event)
		{
			publishEvent<ProjectionChangedEventForPersonScheduleDay>(@event);
		}

		public void Handle(ScheduleInitializeTriggeredEventForScheduleDay @event)
		{
			publishEvent<ProjectionChangedEventForScheduleDay>(@event);
		}

		public void Handle(ScheduleInitializeTriggeredEventForScheduleProjection @event)
		{
			publishEvent<ProjectionChangedEventForScheduleProjection>(@event);
		}

		private void publishEvent<T>(ScheduleChangedEventBase @event) where T : ProjectionChangedEventBase, new()
		{
			var data = getData(@event);
			if (data == null) return;
			_projectionChangedEventBuilder.Build<T>(@event, data.ScheduleRange, data.RealPeriod)
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

			var person = _personRepository.FindPeople(new[] {@event.PersonId}).FirstOrDefault();
			if (person == null)
			{
				Logger.InfoFormat("Person not found (Id: {0})", @event.PersonId);
				return null;
			}

			// gets the date period the same way as FindSchedulesForPersonOnlyInGivenPeriod, hence duplication
			var dateOnlyPeriod = period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()); 
			var dates = dateOnlyPeriod.StartDate.DateRange(dateOnlyPeriod.EndDate);
			_projectionVersionPersister.Upsert(@event.PersonId, dates);
			
			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				scenario);

			var range = schedule[person];

			DateTimePeriod? actualPeriod = @event.SkipDelete ? range.TotalPeriod() : period;

			if (!actualPeriod.HasValue) return null;

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var realPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);

			Logger.DebugFormat("RealPeriod start: {0}, end: {1}", realPeriod.StartDate, realPeriod.EndDate);
			return new range
			{
				ScheduleRange = range,
				RealPeriod = realPeriod,
				ScheduleLoadedTime = schedule.ScheduleLoadedTime
			};
		}
	}
}