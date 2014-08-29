using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class TrackingMessageSender : ITrackingMessageSender
	{
		private readonly IMessageSender _sender;
		private readonly IJsonSerializer _jsonSerializer;

		public TrackingMessageSender(IMessageSender sender, IJsonSerializer jsonSerializer)
		{
			_sender = sender;
			_jsonSerializer = jsonSerializer;
		}

		public void SendTrackingMessage(Guid initiatorId, Guid businessUnitId, TrackingMessage message)
		{
			_sender.Send(new Notification
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