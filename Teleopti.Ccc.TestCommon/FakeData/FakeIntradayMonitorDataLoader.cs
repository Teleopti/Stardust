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
				OfferedCalls = OfferedCalls,
				LatestStatsTime = LatestStatsTime
			};
		}

		public void Has(int offeredCalls, DateTime latestStatsTime)
		{
			OfferedCalls = offeredCalls;
			LatestStatsTime = latestStatsTime;
		}

		public int OfferedCalls { get; set; }
		public DateTime LatestStatsTime { get; set; }
	}
}