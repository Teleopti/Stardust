namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public class IntradayIncomingSummary
	{
		public double ForecastedCalls { get; set; }
		public double ForecastedAverageHandleTime { get; set; }
		public double ForecastedHandleTime { get; set; }
		public double CalculatedCalls { get; set; }
		public double AverageHandleTime { get; set; }
		public double HandleTime { get; set; }
		public double ForecastedActualCallsDiff { get; set; }
		public double ForecastedActualHandleTimeDiff { get; set; }
		public double AverageSpeedOfAnswer { get; set; }
		public double SpeedOfAnswer { get; set; }
		public double AnsweredCalls { get; set; }
		public double ServiceLevel { get; set; }
		public double AnsweredCallsWithinSL { get; set; }
		public double AbandonRate { get; set; }
		public double AbandonedCalls { get; set; }
	}
}