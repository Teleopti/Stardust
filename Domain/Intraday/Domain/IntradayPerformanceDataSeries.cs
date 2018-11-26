using System;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public class IntradayPerformanceDataSeries
	{
		public DateTime[] Time { get; set; }
		public double?[] EstimatedServiceLevels { get; set; }
		public double?[] AverageSpeedOfAnswer { get; set; }
		public double?[] AbandonedRate { get; set; }
		public double?[] ServiceLevel { get; set; }
	}
}