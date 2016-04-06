using System;
using System.Text;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class ServiceBusHealthCheckEventHandler : IHandleEvent<ServiceBusHealthCheckEvent>, IRunOnServiceBus
	{
		private readonly IMessageBrokerComposite _sender;
		private readonly IDeviceInfoProvider _deviceInfo;

		public ServiceBusHealthCheckEventHandler(IMessageBrokerComposite sender, IDeviceInfoProvider deviceInfo)
		{
			_sender = sender;
			_deviceInfo = deviceInfo;
		}

		public void Handle(ServiceBusHealthCheckEvent @event)
		{
			var now = DateTime.UtcNow;
			var deviceInfo = _deviceInfo.GetDeviceInfo();

			var diagnostics = new TeleoptiDiagnosticsInformation
			{
				HandledAt = now,
				SentAt = @event.Timestamp,
				MillisecondsDifference = now.Subtract(@event.Timestamp).Milliseconds,
				Services = deviceInfo.Services,
				MachineName = deviceInfo.MachineName,
				TotalPhysicalMemory = deviceInfo.TotalPhysicalMemory,
				AvailablePhysicalMemory = deviceInfo.AvailablePhysicalMemory,
				BusMemoryConsumption = deviceInfo.BusMemoryConsumption,
				OSFullName = deviceInfo.OSFullName,
				OSPlatform = deviceInfo.OSPlatform,
				OSVersion = deviceInfo.OSVersion
			};

			var binaryData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(diagnostics));
			_sender.Send(@event.LogOnDatasource, @event.LogOnBusinessUnitId, DateTime.Today, DateTime.Today, Guid.Empty,
				@event.InitiatorId, typeof(ITeleoptiDiagnosticsInformation), DomainUpdateType.NotApplicable,
				binaryData);
		}
	}

	public interface IDeviceInfoProvider
	{
		DeviceInfo GetDeviceInfo();
	}

	public class DeviceInfoProvider : IDeviceInfoProvider
	{
		public DeviceInfo GetDeviceInfo()
		{
			throw new NotImplementedException();
		}
	}

	public class DeviceInfo
	{
		public string MachineName { get; set; }
		public long TotalPhysicalMemory { get; set; }
		public long AvailablePhysicalMemory { get; set; }
		public long BusMemoryConsumption { get; set; }
		public string OSFullName { get; set; }
		public string OSPlatform { get; set; }
		public string OSVersion { get; set; }
		public ServiceProcessDetail[] Services { get; set; }
	}
}