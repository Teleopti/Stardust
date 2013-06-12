using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangedHandler : 
		IHandleEvent<ScheduleChangedEvent>, 
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleProjection>,
		IHandleEvent<ScheduleInitializeTriggeredEventForScheduleDay>,
		IHandleEvent<ScheduleInitializeTriggeredEventForPersonScheduleDay>,
		IHandleEvent<FullDayAbsenceAddedEvent>,
		IHandleEvent<PersonAbsenceRemovedEvent>,
		IHandleEvent<PersonAbsenceAddedEvent>
	{
		private readonly IPublishEventsFromEventHandlers _bus;
	    private static readonly ILog Logger = LogManager.GetLogger(typeof (ScheduleChangedHandler));
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IProjectionChangedEventBuilder _projectionChangedEventBuilder;
		private IScheduleRange _range;
		private DateOnlyPeriod _realPeriod;

		public ScheduleChangedHandler(IPublishEventsFromEventHandlers bus, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IScheduleRepository scheduleRepository, IProjectionChangedEventBuilder projectionChangedEventBuilder)
		{
			_bus = bus;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_projectionChangedEventBuilder = projectionChangedEventBuilder;
		    
		}

		public void Handle(PersonAbsenceRemovedEvent @event)
		{
			_bus.Publish(new ScheduleChangedEvent
			{
				Timestamp = @event.Timestamp,
				BusinessUnitId = @event.BusinessUnitId,
				Datasource = @event.Datasource,
				PersonId = @event.PersonId,
				ScenarioId = @event.ScenarioId,
				StartDateTime = @event.StartDateTime,
				EndDateTime = @event.EndDateTime,
			});
		}

		public void Handle(PersonAbsenceAddedEvent @event)
		{
			_bus.Publish(new ScheduleChangedEvent
			{
				Timestamp = @event.Timestamp,
				BusinessUnitId = @event.BusinessUnitId,
				Datasource = @event.Datasource,
				PersonId = @event.PersonId,
				ScenarioId = @event.ScenarioId,
				StartDateTime = @event.StartDateTime,
				EndDateTime = @event.EndDateTime,
			});
		}

		public void Handle(FullDayAbsenceAddedEvent @event)
		{
			_bus.Publish(new ScheduleChangedEvent
				{
					Timestamp = @event.Timestamp,
					BusinessUnitId = @event.BusinessUnitId,
					Datasource = @event.Datasource,
					PersonId = @event.PersonId,
					ScenarioId = @event.ScenarioId,
					StartDateTime = @event.StartDateTime,
					EndDateTime = @event.EndDateTime,
				});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ScheduleChangedEvent @event)
		{
			if (!getPeriodAndScenario(@event)) return;
			_projectionChangedEventBuilder.Build<ProjectionChangedEvent>(@event, _range, _realPeriod, d => _bus.Publish(d));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ScheduleInitializeTriggeredEventForPersonScheduleDay @event)
		{
			if (!getPeriodAndScenario(@event)) return;
			_projectionChangedEventBuilder.Build<ProjectionChangedEventForPersonScheduleDay>(@event, _range, _realPeriod, d => _bus.Publish(d));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ScheduleInitializeTriggeredEventForScheduleDay @event)
		{
			if (!getPeriodAndScenario(@event)) return;
			_projectionChangedEventBuilder.Build<ProjectionChangedEventForScheduleDay>(@event, _range, _realPeriod, d => _bus.Publish(d));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ScheduleInitializeTriggeredEventForScheduleProjection @event)
		{
			if (!getPeriodAndScenario(@event)) return;
			_projectionChangedEventBuilder.Build<ProjectionChangedEventForScheduleProjection>(@event, _range, _realPeriod, d => _bus.Publish(d));
		}

		private bool getPeriodAndScenario(ScheduleChangedEventBase @event)
		{
			var scenario = _scenarioRepository.Get(@event.ScenarioId);
            if (scenario == null)
            {
                Logger.InfoFormat("Scenario not found (Id: {0})", @event.ScenarioId);
                return false;
            }
			if (!scenario.DefaultScenario) return false;

			var period = new DateTimePeriod(@event.StartDateTime, @event.EndDateTime);
			var person = _personRepository.FindPeople(new []{ @event.PersonId}).FirstOrDefault();
		    if (person == null)
		    {
                Logger.InfoFormat("Person not found (Id: {0})", @event.PersonId);
		        return false;
		    }
                    
		    var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);
			var schedule =
				_scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] {person}) {DoLoadByPerson = true},
				                                                   new ScheduleDictionaryLoadOptions(false, false),
				                                                   dateOnlyPeriod.ToDateTimePeriod(timeZone), scenario);

			_range = schedule[person];

			DateTimePeriod? actualPeriod = @event.SkipDelete ? _range.TotalPeriod() : period;

			if (!actualPeriod.HasValue) return false;

			_realPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);
			return true;
		}

	}
}