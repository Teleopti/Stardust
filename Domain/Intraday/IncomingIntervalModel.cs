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
        public double? AnsweredCallsWithinSL { get; set; }
        public double? ServiceLevel { get; set; }
        public double? AbandonedCalls { get; set; }
        public double? AbandonedRate { get; set; }
        public double? SpeedOfAnswer { get; set; }
        public double? AverageSpeedOfAnswer { get; set; }
	    public double? AnsweredCalls { get; set; }
	}
}