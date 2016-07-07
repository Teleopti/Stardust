using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class MonitorDataViewModel
	{
	    public DateTime? LatestActualIntervalStart { get; set; }
        public DateTime? LatestActualIntervalEnd { get; set; }
		public MonitorIntradaySummary Summary { get; set; }
		public MonitorIntradayDataSeries DataSeries { get; set; }
	}

	public class MonitorIntradayDataSeries
	{
	    public double? [] AverageSpeedOfAnswer;
	    public DateTime[] Time { get; set; }
		public double [] ForecastedCalls { get; set; }
		public double [] ForecastedAverageHandleTime { get; set; }
		public double?[] AverageHandleTime { get; set; }
		public double?[] OfferedCalls { get; set; }
	    public double?[] AbandonedRate { get; set; }
	    public double?[] ServiceLevel { get; set; }
	}

	public class MonitorIntradaySummary
	{
		public double ForecastedCalls { get; set; }
		public double ForecastedAverageHandleTime { get; set; }
		public double ForecastedHandleTime { get; set; }
		public double OfferedCalls { get; set; }
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