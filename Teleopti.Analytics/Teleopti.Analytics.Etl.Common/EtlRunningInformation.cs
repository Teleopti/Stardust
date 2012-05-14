using System;
using Teleopti.Analytics.Etl.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common
{
	public class EtlRunningInformation : IEtlRunningInformation
	{
		public string ComputerName { get; set; }

		public DateTime StartTime { get; set; }

		public string JobName { get; set; }

		public bool IsStartedByService { get; set; }
	}
}