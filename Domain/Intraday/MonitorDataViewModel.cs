using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class MonitorDataViewModel
	{
		public double ForecastedCalls { get; set; }
		public double ForecastedAverageHandleTime { get; set; }
		public double OfferedCalls { get; set; }
		public double AverageHandleTime { get; set; }
		public double ForecastedActualCallsDiff { get; set; }
		public double ForecastedActualHandleTimeDiff { get; set; }
		public DateTime LatestStatsTime { get; set; }
	}
}