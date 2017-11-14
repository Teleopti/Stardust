using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradayIncomingViewModel
	{

		public DateTime? FirstIntervalStart { get; set; }
		public DateTime? FirstIntervalEnd { get; set; }
		public DateTime? LatestActualIntervalStart { get; set; }
		public DateTime? LatestActualIntervalEnd { get; set; }
		public IntradayIncomingSummary Summary { get; set; }
		public IntradayIncomingDataSeries DataSeries { get; set; }
		public bool IncomingTrafficHasData { get; set; }
	}
}