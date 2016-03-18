using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayMonitorDataLoader : IIntradayMonitorDataLoader
	{
		public MonitorDataViewModel Load(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			return new MonitorDataViewModel
			{
				ForecastedCalls = ForecastedCalls,
				ForecastedAverageHandleTime = ForecastedAverageHandleTime,
				OfferedCalls = OfferedCalls,
				AverageHandleTime = AverageHandleTime,
				LatestStatsTime = LatestStatsTime,
				ForecastedActualCallsDiff = ForecastedActualCallsDiff,
				ForecastedActualHandleTimeDiff = ForecastedActualHandleTimeDiff
			};
		}

		public void Has(double forecastedCalls, double forecastedAverageHandleTime, double offeredCalls, double averageHandleTime, DateTime latestStatsTime, double forecastedActualCallsDiff, double forecastedActualHandleTimeDiff)
		{
			ForecastedCalls = forecastedCalls;
			ForecastedAverageHandleTime = forecastedAverageHandleTime;
			OfferedCalls = offeredCalls;
			AverageHandleTime = averageHandleTime;
			LatestStatsTime = latestStatsTime;
			ForecastedActualCallsDiff = forecastedActualCallsDiff;
			ForecastedActualHandleTimeDiff = forecastedActualHandleTimeDiff;
		}

		public double ForecastedCalls { get; set; }
		public double ForecastedAverageHandleTime { get; set; }
		public double OfferedCalls { get; set; }
		public double AverageHandleTime { get; set; }
		public DateTime LatestStatsTime { get; set; }
		public double ForecastedActualCallsDiff { get; set; }
		public double ForecastedActualHandleTimeDiff { get; set; }
	}
}