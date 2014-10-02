using System;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages;

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

		public void SendTrackingMessage(IRaptorDomainMessageInfo originatingEvent, TrackingMessage message)
		{
			_sender.Send(new Notification
			{
				DataSource = originatingEvent.Datasource,
				BusinessUnitId = originatingEvent.BusinessUnitId.ToString(),
				ModuleId = originatingEvent.InitiatorId.ToString(),
				BinaryData = _jsonSerializer.SerializeObject(message),
				DomainId = message.TrackId.ToString(),
				DomainType = "TrackingMessage",
				DomainReferenceId = originatingEvent.InitiatorId.ToString()
			});
		}
	}
}