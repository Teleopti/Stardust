using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayQueueStatisticsLoader : IIntradayQueueStatisticsLoader
	{
		private IList<SkillIntervalCalls> _latestStatisticsTimeAndWorkload = new List<SkillIntervalCalls>();

		public IList<SkillIntervalCalls> LoadActualCallPerSkillInterval(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			return _latestStatisticsTimeAndWorkload;
		}

		public void Has(IList<SkillIntervalCalls> latestStatisticsTimeAndWorkload)
		{
			_latestStatisticsTimeAndWorkload = latestStatisticsTimeAndWorkload;
		}
	}
}