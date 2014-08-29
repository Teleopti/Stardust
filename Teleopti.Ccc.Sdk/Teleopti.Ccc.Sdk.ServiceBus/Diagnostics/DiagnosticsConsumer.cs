using System;
using System.Text;
using Newtonsoft.Json;
using Rhino.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Diagnostics
{
	public class DiagnosticsConsumer : ConsumerOf<DiagnosticsMessage>
	{
		private readonly IMessageBroker _broker;

		public DiagnosticsConsumer(IMessageBroker broker)
		{
			_broker = broker;
		}

		public void Consume(DiagnosticsMessage message)
		{
			var now = DateTime.UtcNow;
			var diagnostics = new TeleoptiDiagnosticsInformation
			{
				HandledAt = now,
				SentAt = message.Timestamp,
				MillisecondsDifference = now.Subtract(message.Timestamp).Milliseconds
			};

			var binaryData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(diagnostics));
			_broker.SendEventMessage(message.Datasource, message.BusinessUnitId, DateOnly.Today, DateOnly.Today, Guid.Empty,
				message.InitiatorId, typeof (ITeleoptiDiagnosticsInformation), DomainUpdateType.NotApplicable,
				binaryData);
		}
	}
}
