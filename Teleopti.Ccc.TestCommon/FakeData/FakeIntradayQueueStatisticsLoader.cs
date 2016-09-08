using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayQueueStatisticsLoader : IIntradayQueueStatisticsLoader
	{
		private LatestStatisticsTimeAndWorkload _latestStatisticsTimeAndWorkload;

		public LatestStatisticsTimeAndWorkload LoadActualWorkloadInSeconds(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			return _latestStatisticsTimeAndWorkload;
		}

		public void Has(LatestStatisticsTimeAndWorkload latestStatisticsTimeAndWorkload)
		{
			_latestStatisticsTimeAndWorkload = latestStatisticsTimeAndWorkload;
		}
	}
}