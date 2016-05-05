using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Messages.Denormalize;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBus.Legacy
{
	public class LegacyMessageTransformer :
		ConsumerOf<ScheduleChanged>,
		ConsumerOf<DenormalizeScheduleProjection>,
		ConsumerOf<NewAbsenceRequestCreated>,
		ConsumerOf<GroupPageChangedMessage>,
		ConsumerOf<PersonPeriodChangedMessage>,
		ConsumerOf<PersonChangedMessage>, 
		ConsumerOf<NewShiftTradeRequestCreated>,
		ConsumerOf<NewAbsenceReportCreated>,
		ConsumerOf<AcceptShiftTrade>,
		ConsumerOf<FullDayAbsenceAddedEvent>,
		ConsumerOf<PersonAbsenceAddedEvent>,
		ConsumerOf<ActivityAddedEvent>,
		ConsumerOf<ActivityMovedEvent>,
		ConsumerOf<PersonAbsenceModifiedEvent>,
		ConsumerOf<DayOffAddedEvent>,
		ConsumerOf<DayUnscheduledEvent>,
		ConsumerOf<PersonAssignmentLayerRemovedEvent>
	{
		private readonly IEventPublisher _publisher;

		public LegacyMessageTransformer(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Consume(DenormalizeScheduleProjection message)
		{
			_publisher.Publish(new ScheduleChangedEvent
			{
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				PersonId = message.PersonId,
				ScenarioId = message.ScenarioId,
				SkipDelete = message.SkipDelete,
				StartDateTime = message.StartDateTime,
				EndDateTime = message.EndDateTime,
				Timestamp = message.Timestamp
			});
		}

		public void Consume(ScheduleChanged message)
		{
			_publisher.Publish(new ScheduleChangedEvent
			{
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				PersonId = message.PersonId,
				ScenarioId = message.ScenarioId,
				SkipDelete = message.SkipDelete,
				StartDateTime = message.StartDateTime,
				EndDateTime = message.EndDateTime,
				Timestamp = message.Timestamp
			});
		}

		public void Consume(NewAbsenceRequestCreated message)
		{
			_publisher.Publish(new NewAbsenceRequestCreatedEvent
			{
				InitiatorId = message.InitiatorId,
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				PersonRequestId = message.PersonRequestId,
				Timestamp = message.Timestamp,
				JobName = "Absence Request",
				UserName = message.InitiatorId.ToString()
			});
		}

		public void Consume(GroupPageChangedMessage message)
		{
			_publisher.Publish(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				InitiatorId = message.InitiatorId,
				SerializedGroupPage = message.SerializedGroupPage,
				Timestamp = message.Timestamp
			});
		}

		public void Consume(PersonPeriodChangedMessage message)
		{
			_publisher.Publish(new SettingsForPersonPeriodChangedEvent
			{
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				InitiatorId = message.InitiatorId,
				SerializedIds = message.SerializedPersonPeriod,
				Timestamp = message.Timestamp
			});
		}

		public void Consume(PersonChangedMessage message)
		{
			_publisher.Publish(new PersonCollectionChangedEvent
			{
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				InitiatorId = message.InitiatorId,
				SerializedPeople = message.SerializedPeople,
				Timestamp = message.Timestamp
			});
		}


		public void Consume(NewShiftTradeRequestCreated message)
		{
			_publisher.Publish(new NewShiftTradeRequestCreatedEvent
			{
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				InitiatorId = message.InitiatorId,
				Timestamp = message.Timestamp,
				PersonRequestId = message.PersonRequestId
			});
		}

		public void Consume(AcceptShiftTrade message)
		{
			_publisher.Publish(new AcceptShiftTradeEvent
			{
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				InitiatorId = message.InitiatorId,
				Timestamp = message.Timestamp,
				PersonRequestId = message.PersonRequestId,
				AcceptingPersonId = message.AcceptingPersonId,
				Message = message.Message
			});
		}

		public void Consume(FullDayAbsenceAddedEvent message)
		{
			_publisher.Publish(message);
		}

		public void Consume(PersonAbsenceAddedEvent message)
		{
			_publisher.Publish(message);
		}

		public void Consume(ActivityAddedEvent message)
		{
			_publisher.Publish(message);
		}

		public void Consume(ActivityMovedEvent message)
		{
			_publisher.Publish(message);
		}

		public void Consume(PersonAbsenceModifiedEvent message)
		{
			_publisher.Publish(message);
		}

		public void Consume(DayOffAddedEvent message)
		{
			_publisher.Publish(message);
		}

		public void Consume(DayUnscheduledEvent message)
		{
			_publisher.Publish(message);
		}

		public void Consume(PersonAssignmentLayerRemovedEvent message)
		{
			_publisher.Publish(message);
		}
		
		public void Consume(NewAbsenceReportCreated message)
		{
			_publisher.Publish(new NewAbsenceReportCreatedEvent
			{
				AbsenceId = message.AbsenceId,
				InitiatorId = message.InitiatorId,
				JobName = "Absence Report",
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				PersonId = message.PersonId,
				RequestedDate = message.RequestedDate,
				Timestamp = message.Timestamp,
				UserName = message.InitiatorId.ToString()
			});
		}

	}
}