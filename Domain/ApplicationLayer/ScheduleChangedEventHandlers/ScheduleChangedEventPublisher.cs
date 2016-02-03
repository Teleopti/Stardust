using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangedEventPublisher :
		IHandleEvent<FullDayAbsenceAddedEvent>,
		IHandleEvent<PersonAbsenceRemovedEvent>,
		IHandleEvent<PersonAbsenceAddedEvent>,
		IHandleEvent<ActivityAddedEvent>,
		IHandleEvent<ActivityMovedEvent>,
		IHandleEvent<PersonAbsenceModifiedEvent>,
		IHandleEvent<DayOffAddedEvent>,
		IHandleEvent<DayUnscheduledEvent>,
		IRunOnServiceBus
	{
		private readonly IEventPublisher _publisher;

		public ScheduleChangedEventPublisher(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Handle(PersonAbsenceRemovedEvent @event)
		{
			_publisher.Publish(new ScheduleChangedEvent
				{
					Timestamp = @event.Timestamp,
					LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
					LogOnDatasource = @event.LogOnDatasource,
					PersonId = @event.PersonId,
					ScenarioId = @event.ScenarioId,
					StartDateTime = @event.StartDateTime,
					EndDateTime = @event.EndDateTime,
					InitiatorId = @event.InitiatorId,
					TrackId = @event.TrackId
				});
		}

		public void Handle(PersonAbsenceAddedEvent @event)
		{
			_publisher.Publish(new ScheduleChangedEvent
				{
					Timestamp = @event.Timestamp,
					LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
					LogOnDatasource = @event.LogOnDatasource,
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
					LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
					LogOnDatasource = @event.LogOnDatasource,
					PersonId = @event.PersonId,
					ScenarioId = @event.ScenarioId,
					StartDateTime = @event.StartDateTime,
					EndDateTime = @event.EndDateTime,
					InitiatorId = @event.InitiatorId,
					TrackId = @event.TrackId
				});
		}

		public void Handle(ActivityAddedEvent @event)
		{
			_publisher.Publish(new ScheduleChangedEvent
				{
					Timestamp = @event.Timestamp,
					LogOnDatasource = @event.LogOnDatasource,
					LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
					PersonId = @event.PersonId,
					ScenarioId = @event.ScenarioId,
					StartDateTime = @event.StartDateTime,
					EndDateTime = @event.EndDateTime,
					InitiatorId = @event.InitiatorId,
					TrackId = @event.TrackId
				});
		}

		public void Handle(ActivityMovedEvent @event)
		{
			_publisher.Publish(new ScheduleChangedEvent
			{
				Timestamp = @event.Timestamp,
				LogOnDatasource = @event.LogOnDatasource,
				LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
				PersonId = @event.PersonId,
				ScenarioId = @event.ScenarioId,
				StartDateTime = @event.StartDateTime,
				EndDateTime = @event.EndDateTime,
				InitiatorId = @event.InitiatorId,
				TrackId = @event.TrackId
			});
		}

		public void Handle (PersonAbsenceModifiedEvent @event)
		{
			_publisher.Publish(new ScheduleChangedEvent
			{
				Timestamp = @event.Timestamp,
				LogOnDatasource = @event.LogOnDatasource,
				LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
				PersonId = @event.PersonId,
				ScenarioId = @event.ScenarioId,
				StartDateTime = @event.StartDateTime,
				EndDateTime = @event.EndDateTime,
				InitiatorId = @event.InitiatorId,
				TrackId = @event.TrackId
			});
		}

		public void Handle(DayOffAddedEvent @event)
		{
			var dateTimeperiod = dateOnlyToScheduleChangedPeriodWithOverflowBecauseWeAreScared(@event.Date);
			_publisher.Publish(new ScheduleChangedEvent
			{
				Timestamp = @event.Timestamp,
				LogOnDatasource = @event.LogOnDatasource,
				LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
				PersonId = @event.PersonId,
				ScenarioId = @event.ScenarioId,
				StartDateTime = dateTimeperiod.StartDateTime,
				EndDateTime = dateTimeperiod.EndDateTime,
				InitiatorId = @event.InitiatorId
			});
		}

		public void Handle(DayUnscheduledEvent @event)
		{
			var dateTimeperiod = dateOnlyToScheduleChangedPeriodWithOverflowBecauseWeAreScared(@event.Date);
			_publisher.Publish(new ScheduleChangedEvent
			{
				Timestamp = @event.Timestamp,
				LogOnDatasource = @event.LogOnDatasource,
				LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
				PersonId = @event.PersonId,
				ScenarioId = @event.ScenarioId,
				StartDateTime = dateTimeperiod.StartDateTime,
				EndDateTime = dateTimeperiod.EndDateTime,
				InitiatorId = @event.InitiatorId
			});
		}

		private static DateTimePeriod dateOnlyToScheduleChangedPeriodWithOverflowBecauseWeAreScared(DateOnly date)
		{
			var dateTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
			return new DateTimePeriod(
				dateTime.AddHours(-24),
				dateTime.AddHours(48));
		}

	}
}