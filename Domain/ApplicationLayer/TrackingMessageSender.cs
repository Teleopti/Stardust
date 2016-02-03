using System;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
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

		public void SendTrackingMessage(IEvent originatingEvent, TrackingMessage message)
		{
			string dataSource = null;
			var businessUnitId = Guid.Empty;
			var initiatorId = Guid.Empty;

			var logOnInfo = originatingEvent as ILogOnInfo;
			if (logOnInfo != null)
			{
				dataSource = logOnInfo.LogOnDatasource;
				businessUnitId = logOnInfo.LogOnBusinessUnitId;
			}
			var initiatorInfo = originatingEvent as IInitiatorInfo;
			if (initiatorInfo != null)
			{
				initiatorId = initiatorInfo.InitiatorId;
			}
			_sender.Send(new Interfaces.MessageBroker.Message
			{
				DataSource = dataSource,
				BusinessUnitId = businessUnitId.ToString(),
				ModuleId = initiatorId.ToString(),
				BinaryData = _jsonSerializer.SerializeObject(message),
				DomainId = message.TrackId.ToString(),
				DomainType = "TrackingMessage",
				DomainReferenceId = initiatorId.ToString()
			});
		}
	}
}