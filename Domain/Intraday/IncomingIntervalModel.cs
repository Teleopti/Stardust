namespace Teleopti.Ccc.Domain.Intraday
{
	public class IncomingIntervalModel
	{
		public int IntervalId { get; set; }
		public double ForecastedCalls { get; set; }
		public double ForecastedHandleTime { get; set; }
		public double ForecastedAverageHandleTime { get; set; }
		public double? OfferedCalls { get; set; }
		public double? HandleTime { get; set; }
		public double? AverageHandleTime { get; set; }
	}
}