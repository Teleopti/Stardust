using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class MonitorDataViewModel
	{
		public int ForecastedCalls { get; set; }
		public int OfferedCalls { get; set; }
		public DateTime LatestStatsTime { get; set; }
	}
}