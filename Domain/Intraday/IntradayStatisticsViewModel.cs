using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradayStatisticsViewModel
	{
	    public DateTime? LatestActualIntervalStart { get; set; }
        public DateTime? LatestActualIntervalEnd { get; set; }
		public IntradayStatisticsSummary StatisticsSummary { get; set; }
		public IntradayStatisticsDataSeries StatisticsDataSeries { get; set; }
	}
}