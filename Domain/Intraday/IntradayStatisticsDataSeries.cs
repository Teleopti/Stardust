using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradayStatisticsDataSeries
	{
		public double? [] AverageSpeedOfAnswer;
		public DateTime[] Time { get; set; }
		public double [] ForecastedCalls { get; set; }
		public double [] ForecastedAverageHandleTime { get; set; }
		public double?[] AverageHandleTime { get; set; }
		public double?[] OfferedCalls { get; set; }
		public double?[] AbandonedRate { get; set; }
		public double?[] ServiceLevel { get; set; }
	}
}