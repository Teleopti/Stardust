using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class MonitorDataViewModel
	{
		public double ForecastedCalls { get; set; }
		public double OfferedCalls { get; set; }
		public DateTime LatestStatsTime { get; set; }
		public double ForecastedActualCallsDiff { get; set; }
	}
}