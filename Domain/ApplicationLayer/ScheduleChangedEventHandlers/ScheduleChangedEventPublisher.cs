using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
		IHandleEvent<DayOffDeletedEvent>,
		IHandleEvent<DayUnscheduledEvent>,
		IHandleEvent<PersonAssignmentLayerRemovedEvent>,
		IHandleEvent<MainShiftCategoryReplaceEvent>,
		IHandleEvent<PersonalActivityAddedEvent>,
		IHandleEvent<ScheduleBackoutEvent>,
		IRunOnHangfire
	{
		private readonly IEventPublisher _publisher;

		public ScheduleChangedEventPublisher(IEventPublisher publisher)
		{
			_publisher = publisher;
		}
		
		public void Handle(PersonalActivityAddedEvent @event)
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
				CommandId = @event.CommandId
			});
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
					CommandId = @event.CommandId
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
					CommandId = @event.CommandId
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
					CommandId = @event.CommandId
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
					CommandId = @event.CommandId,
					Date = @event.Date
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
				CommandId = @event.CommandId
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
				CommandId = @event.CommandId
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
				InitiatorId = @event.InitiatorId,
				CommandId = @event.CommandId
			});
		}
		public void Handle(DayOffDeletedEvent @event)
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
				InitiatorId = @event.InitiatorId,
				CommandId = @event.CommandId
			});
		}
		public void Handle(MainShiftCategoryReplaceEvent @event)
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
				InitiatorId = @event.InitiatorId,
				CommandId = @event.CommandId
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
				InitiatorId = @event.InitiatorId,
				CommandId = @event.CommandId
			});
		}

		private static DateTimePeriod dateOnlyToScheduleChangedPeriodWithOverflowBecauseWeAreScared(DateTime date)
		{
			var dateTime = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
			return new DateTimePeriod(
				dateTime.AddHours(-24),
				dateTime.AddHours(48));
		}

		public void Handle(PersonAssignmentLayerRemovedEvent @event)
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
				CommandId = @event.CommandId,
				Date = @event.Date
			});
		}

		public void Handle(ScheduleBackoutEvent @event)
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
				CommandId = @event.CommandId
			});
		}
	}
}