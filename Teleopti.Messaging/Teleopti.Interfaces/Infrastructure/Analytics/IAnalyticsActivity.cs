using System;

namespace Teleopti.Interfaces.Infrastructure.Analytics
{
	public interface IAnalyticsActivity
	{
		int ActivityId { get; set; }
		Guid ActivityCode { get; set; } 
		bool InPaidTime { get; set; }
		bool InReadyTime { get; set; } 
	}
}