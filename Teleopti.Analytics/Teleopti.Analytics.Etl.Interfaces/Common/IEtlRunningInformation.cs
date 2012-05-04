using System;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public interface IEtlRunningInformation
	{
		string ComputerName { get; set; }
		DateTime StartTime { get; set; }
		string JobName { get; set; }
		bool IsStartedByService { get; set; }
	}
}