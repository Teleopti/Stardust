using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Diagnostics
{
	public class DiagnosticsConsumer : ConsumerOf<DiagnosticsMessage>
	{
		private readonly IMessageBrokerComposite _broker;

		public DiagnosticsConsumer(IMessageBrokerComposite broker)
		{
			_broker = broker;
		}

		public void Consume(DiagnosticsMessage message)
		{
			var now = DateTime.UtcNow;
			var computerInfo = new ServerComputer();
			var services = System.ServiceProcess.ServiceController.GetServices();
			var process = Process.GetCurrentProcess();

			var diagnostics = new TeleoptiDiagnosticsInformation
			{
				HandledAt = now,
				SentAt = message.Timestamp,
				MillisecondsDifference = now.Subtract(message.Timestamp).Milliseconds,
				Services = services.Where(s => s.ServiceName.Contains("Teleopti")).Select(s => new ServiceProcessDetail{Name= s.DisplayName,Status = (int) s.Status}).ToArray(),
				MachineName = computerInfo.Name,
				TotalPhysicalMemory = (long) computerInfo.Info.TotalPhysicalMemory,
				AvailablePhysicalMemory = (long) computerInfo.Info.AvailablePhysicalMemory,
				BusMemoryConsumption = process.WorkingSet64,
				OSFullName = computerInfo.Info.OSFullName,
				OSPlatform = computerInfo.Info.OSPlatform,
				OSVersion = computerInfo.Info.OSVersion,
			};

			var binaryData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(diagnostics));
			_broker.Send(message.LogOnDatasource, message.LogOnBusinessUnitId, DateTime.Today, DateTime.Today, Guid.Empty,
				message.InitiatorId, typeof (ITeleoptiDiagnosticsInformation), DomainUpdateType.NotApplicable,
				binaryData);
		}
	}
}
