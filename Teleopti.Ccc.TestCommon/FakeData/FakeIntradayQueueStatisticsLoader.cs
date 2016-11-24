using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayQueueStatisticsLoader : IIntradayQueueStatisticsLoader
	{
		private IList<SkillIntervalStatistics> _latestStatisticsTimeAndWorkload = new List<SkillIntervalStatistics>();

		public IList<SkillIntervalStatistics> LoadActualCallPerSkillInterval(IList<ISkill> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			return _latestStatisticsTimeAndWorkload;
		}

		public void Has(IList<SkillIntervalStatistics> latestStatisticsTimeAndWorkload)
		{
			_latestStatisticsTimeAndWorkload = latestStatisticsTimeAndWorkload;
		}
	}
}