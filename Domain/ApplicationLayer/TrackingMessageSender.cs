using System;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;

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

			if (originatingEvent is ILogOnContext logOnInfo)
			{
				dataSource = logOnInfo.LogOnDatasource;
				businessUnitId = logOnInfo.LogOnBusinessUnitId;
			}

			if (originatingEvent is IInitiatorContext initiatorInfo)
			{
				initiatorId = initiatorInfo.InitiatorId;
			}
			_sender.Send(new Message
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