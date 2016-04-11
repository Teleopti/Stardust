using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBus.Legacy
{
	//Only here to convert "old" messages still in the database when patching a customer
	public class LegacyAbsenceRequestConsumer : ConsumerOf<NewAbsenceRequestCreated>
	{
		private readonly IEventPublisher _eventPublisher;

		public LegacyAbsenceRequestConsumer(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Consume(NewAbsenceRequestCreated message)
		{
			var @event = new NewAbsenceRequestCreatedEvent
			{
				InitiatorId = message.InitiatorId,
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				LogOnDatasource = message.LogOnDatasource,
				PersonRequestId = message.PersonRequestId,
				Timestamp = message.Timestamp,
				JobName = "Absence Request",
				Type = typeof(NewAbsenceRequestCreatedEvent).ToString()
			};
			_eventPublisher.Publish(@event);
		}
	}
}