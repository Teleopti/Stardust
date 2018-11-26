using System;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels
{
	public class IntradayPerformanceViewModel
	{
		public IntradayPerformanceDataSeries DataSeries { get; set; }
		public IntradayPerformanceSummary Summary { get; set; }
		public DateTime? LatestActualIntervalStart { get; set; }
		public DateTime? LatestActualIntervalEnd { get; set; }
		public bool PerformanceHasData { get; set; }
	}
}