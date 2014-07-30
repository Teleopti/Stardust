using System;
using Newtonsoft.Json;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.SignalR
{
	public class EventTracker : IEventTracker
	{
		private readonly IMessageBrokerSender _messageBroker;

		public EventTracker(IMessageBrokerSender messageBroker)
		{
			_messageBroker = messageBroker;
		}

		public void SendTrackingMessage(Guid initiatorId, Guid businessUnitId, TrackingMessage message)
		{
			_messageBroker.SendNotification(new Notification
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