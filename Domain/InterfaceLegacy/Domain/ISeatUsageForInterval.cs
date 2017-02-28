using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatUsageForInterval
	{
		DateTime IntervalStart { get; set; }
		DateTime IntervalEnd { get; set; }
		int SeatUsage { get; set; }
	}
}
