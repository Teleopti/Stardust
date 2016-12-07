using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradayIncomingViewModel
	{
	    public DateTime? LatestActualIntervalStart { get; set; }
        public DateTime? LatestActualIntervalEnd { get; set; }
		public IntradayIncomingSummary IncomingSummary { get; set; }
		public IntradayIncomingDataSeries IncomingDataSeries { get; set; }
	}
}