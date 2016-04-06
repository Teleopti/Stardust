using System.Diagnostics;
using System.Linq;
using Microsoft.VisualBasic.Devices;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.HealthCheck
{
	public class DeviceInfoProvider : IDeviceInfoProvider
	{
		public DeviceInfo GetDeviceInfo()
		{
			var computerInfo = new ServerComputer();
			var process = Process.GetCurrentProcess();
			return new DeviceInfo
			{
				MachineName = computerInfo.Name,
				TotalPhysicalMemory = (long)computerInfo.Info.TotalPhysicalMemory,
				AvailablePhysicalMemory = (long)computerInfo.Info.AvailablePhysicalMemory,
				BusMemoryConsumption = process.WorkingSet64,
				OSFullName = computerInfo.Info.OSFullName,
				OSPlatform = computerInfo.Info.OSPlatform,
				OSVersion = computerInfo.Info.OSVersion,
				Services = System.ServiceProcess.ServiceController.GetServices()
					.Where(s => s.ServiceName.Contains("Teleopti"))
					.Select(s => new ServiceProcessDetail { Name = s.DisplayName, Status = (int)s.Status })
					.ToArray()
			};
		}
	}
}