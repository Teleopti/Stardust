using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventPublisher : 
		IHandleEvent<ScheduleChangedEvent>, 
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleProjection>,
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleDay>,
		IHandleEvent<ScheduleInitializeTriggeredEventForPersonScheduleDay>,
		IRunOnServiceBus
	{
		private readonly IPublishEventsFromEventHandlers _publisher;
	    private static readonly ILog Logger = LogManager.GetLogger(typeof (ProjectionChangedEventPublisher));
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IProjectionChangedEventBuilder _projectionChangedEventBuilder;

		public ProjectionChangedEventPublisher(IPublishEventsFromEventHandlers publisher, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IScheduleRepository scheduleRepository, IProjectionChangedEventBuilder projectionChangedEventBuilder)
		{
			_publisher = publisher;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_projectionChangedEventBuilder = projectionChangedEventBuilder;
		}

		public void Handle(ScheduleChangedEvent @event)
		{
			publishEvent<ProjectionChangedEvent>(@event);
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
										  .ForEach(e => _publisher.Publish(e));
		}

		private class range
		{
			public IScheduleRange ScheduleRange;
			public DateOnlyPeriod RealPeriod;
		}

		private range getData(ScheduleChangedEventBase @event)
		{
			var scenario = _scenarioRepository.Get(@event.ScenarioId);
            if (scenario == null)
            {
                Logger.InfoFormat("Scenario not found (Id: {0})", @event.ScenarioId);
                return null;
            }
			if (!scenario.DefaultScenario) return null;

			var period = new DateTimePeriod(@event.StartDateTime.Subtract(TimeSpan.FromDays(1)), @event.EndDateTime);
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Period start: {0}, end: {1}", period.StartDateTime, period.EndDateTime);
			}
			var person = _personRepository.FindPeople(new []{ @event.PersonId}).FirstOrDefault();
		    if (person == null)
		    {
                Logger.InfoFormat("Person not found (Id: {0})", @event.PersonId);
		        return null;
		    }

			var schedule = _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(person,
				                                                   new ScheduleDictionaryLoadOptions(false, false),
				                                                   period, 
																   scenario);

			var range = schedule[person];

			DateTimePeriod? actualPeriod = @event.SkipDelete ? range.TotalPeriod() : period;

			if (!actualPeriod.HasValue) return null;

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var realPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("RealPeriod start: {0}, end: {1}", realPeriod.StartDate, realPeriod.EndDate);
			}
			return new range
				{
					ScheduleRange = range,
					RealPeriod = realPeriod
				};
		}

	}
}