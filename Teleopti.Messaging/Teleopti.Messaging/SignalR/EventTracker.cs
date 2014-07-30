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

		public void SendTrackingMessage(Guid initiatorId, Guid businessUnitId, Guid trackId)
		{
			_messageBroker.SendNotification(new Notification
			{
				BinaryData = JsonConvert.SerializeObject(new TrackingMessage { TrackId = trackId }),
				BusinessUnitId = businessUnitId.ToString(),
				DomainId = trackId.ToString(),
				DomainType = "TrackingMessage",
				DomainReferenceId = initiatorId.ToString()
			});
		}
	}
}