using System;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels
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