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
		ConsumerOf<PersonPeriodChangedMessage>
	{
		private readonly IServiceBus _bus;
		private readonly IEventPublisher _eventPublisher;

		public LegacyMessageTransformer(IServiceBus bus, IEventPublisher eventPublisher)
		{
			_bus = bus;
			_eventPublisher = eventPublisher;
		}

		public void Consume(DenormalizeScheduleProjection message)
		{
			_bus.SendToSelf(new ScheduleChangedEvent
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
			_bus.SendToSelf(new ScheduleChangedEvent
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
			_eventPublisher.Publish(new NewAbsenceRequestCreatedEvent
			{
				InitiatorId = message.InitiatorId,
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				PersonRequestId = message.PersonRequestId,
				Timestamp = message.Timestamp,
				JobName = "Absence Request",
				Type = typeof (NewAbsenceRequestCreatedEvent).ToString()
			});
		}

		public void Consume(GroupPageChangedMessage message)
		{
			_eventPublisher.Publish(new GroupPageCollectionChangedEvent
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
			_eventPublisher.Publish(new SettingsForPersonPeriodChangedEvent
			{
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				InitiatorId = message.InitiatorId,
				SerializedIds = message.SerializedPersonPeriod,
				Timestamp = message.Timestamp
			});
		}
	}
}