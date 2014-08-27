using System;
using Newtonsoft.Json;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.SignalR
{
	public class TrackingMessageSender : ITrackingMessageSender
	{
		private readonly IMessageBrokerSender _messageBroker;

		public TrackingMessageSender(IMessageBrokerSender messageBroker)
		{
			_messageBroker = messageBroker;
		}

		public void SendTrackingMessage(Guid initiatorId, Guid businessUnitId, TrackingMessage message)
		{
			_messageBroker.Send(new Notification
			{
				BinaryData = JsonConvert.SerializeObject(message),
				BusinessUnitId = businessUnitId.ToString(),
				DomainId = message.TrackId.ToString(),
				DomainType = "TrackingMessage",
				DomainReferenceId = initiatorId.ToString()
			});
		}
	}
}