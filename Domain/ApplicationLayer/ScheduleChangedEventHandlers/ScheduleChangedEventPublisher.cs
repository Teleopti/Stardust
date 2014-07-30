using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangedEventPublisher :
		IHandleEvent<FullDayAbsenceAddedEvent>,
		IHandleEvent<PersonAbsenceRemovedEvent>,
		IHandleEvent<PersonAbsenceAddedEvent>,
		IHandleEvent<ActivityAddedEvent>,
		IHandleEvent<ActivityMovedEvent>


	{
		private readonly IPublishEventsFromEventHandlers _publisher;

		public ScheduleChangedEventPublisher(IPublishEventsFromEventHandlers publisher)
		{
			_publisher = publisher;
		}

		public void Handle(PersonAbsenceRemovedEvent @event)
		{
			_publisher.Publish(new ScheduleChangedEvent
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
			_publisher.Publish(new ScheduleChangedEvent
				{
					Timestamp = @event.Timestamp,
					BusinessUnitId = @event.BusinessUnitId,
					Datasource = @event.Datasource,
					PersonId = @event.PersonId,
					ScenarioId = @event.ScenarioId,
					StartDateTime = @event.StartDateTime,
					EndDateTime = @event.EndDateTime,
					InitiatorId = @event.InitiatorId,
					TrackId = @event.TrackId
				});
		}

		public void Handle(FullDayAbsenceAddedEvent @event)
		{
			_publisher.Publish(new ScheduleChangedEvent
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

		public void Handle(ActivityAddedEvent @event)
		{
			_publisher.Publish(new ScheduleChangedEvent
				{
					Timestamp = @event.Timestamp,
					Datasource = @event.Datasource,
					BusinessUnitId = @event.BusinessUnitId,
					PersonId = @event.PersonId,
					ScenarioId = @event.ScenarioId,
					StartDateTime = @event.StartDateTime,
					EndDateTime = @event.EndDateTime
				});
		}

		public void Handle(ActivityMovedEvent @event)
		{
			_publisher.Publish(new ScheduleChangedEvent
			{
				Timestamp = @event.Timestamp,
				Datasource = @event.Datasource,
				BusinessUnitId = @event.BusinessUnitId,
				PersonId = @event.PersonId,
				ScenarioId = @event.ScenarioId,
				StartDateTime = @event.StartDateTime,
				EndDateTime = @event.EndDateTime
			});
		}
	}
}