using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradayPerformanceViewModel
	{
		public IntradayPerformanceDataSeries DataSeries { get; set; }
		public IntradayPerformanceSummary Summary { get; set; }
		public DateTime? LatestActualIntervalStart { get; set; }
		public DateTime? LatestActualIntervalEnd { get; set; }
	}
}