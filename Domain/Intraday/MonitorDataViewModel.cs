using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class MonitorDataViewModel
	{
		public DateTime? LatestActualInterval { get; set; }
		public MonitorIntradaySummary Summary { get; set; }
		public MonitorIntradayDataSeries DataSeries { get; set; }	
	}

	public class MonitorIntradayDataSeries
	{
		public DateTime[] Time { get; set; }
		public double [] ForecastedCalls { get; set; }
		public double [] ForecastedAverageHandleTime { get; set; }
		public double?[] AverageHandleTime { get; set; }
		public double?[] OfferedCalls { get; set; }
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
		
	}
}