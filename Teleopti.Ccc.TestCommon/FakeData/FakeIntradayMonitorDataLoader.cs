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
				OfferedCalls = OfferedCalls,
				LatestStatsTime = LatestStatsTime,
				ForecastedActualCallsDiff = ForecastedActualCallsDiff
			};
		}

		public void Has(double forecastedCalls, double offeredCalls, DateTime latestStatsTime, double forecastedActualCallsDiff)
		{
			ForecastedCalls = forecastedCalls;
			OfferedCalls = offeredCalls;
			LatestStatsTime = latestStatsTime;
			ForecastedActualCallsDiff = forecastedActualCallsDiff;

		}

		public double ForecastedCalls { get; set; }
		public double OfferedCalls { get; set; }
		public DateTime LatestStatsTime { get; set; }
		public double ForecastedActualCallsDiff { get; set; }
	}
}