using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public class TeleoptiDiagnosticsInformation : ITeleoptiDiagnosticsInformation
	{
		public DateTime HandledAt { get; set; }
		public DateTime SentAt { get; set; }
		public int MillisecondsDifference { get; set; }
		public ICollection<ServiceProcessDetail> Services { get; set; }
		public string MachineName { get; set; }
		public long TotalPhysicalMemory { get; set; }
		public long AvailablePhysicalMemory { get; set; }
		public long BusMemoryConsumption { get; set; }
		public string OSVersion { get; set; }
		public string OSPlatform { get; set; }
		public string OSFullName { get; set; }
	}

	public class ServiceProcessDetail
	{
		public string Name { get; set; }
		public int Status { get; set; }
	}
}