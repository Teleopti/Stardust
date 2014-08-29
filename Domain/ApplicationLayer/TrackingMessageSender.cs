using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class TrackingMessageSender : ITrackingMessageSender
	{
		private readonly IMessageBrokerSender _messageBroker;
		private readonly IJsonSerializer _jsonSerializer;

		public TrackingMessageSender(IMessageBrokerSender messageBroker, IJsonSerializer jsonSerializer)
		{
			_messageBroker = messageBroker;
			_jsonSerializer = jsonSerializer;
		}

		public void SendTrackingMessage(Guid initiatorId, Guid businessUnitId, TrackingMessage message)
		{
			_messageBroker.Send(new Notification
			{
				BinaryData = _jsonSerializer.SerializeObject(message),
				BusinessUnitId = businessUnitId.ToString(),
				DomainId = message.TrackId.ToString(),
				DomainType = "TrackingMessage",
				DomainReferenceId = initiatorId.ToString()
			});
		}
	}
}