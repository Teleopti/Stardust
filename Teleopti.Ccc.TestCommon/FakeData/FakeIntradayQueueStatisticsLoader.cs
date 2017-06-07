using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayQueueStatisticsLoader : IIntradayQueueStatisticsLoader
	{
		private IList<SkillIntervalStatistics> _latestStatisticsTimeAndWorkload = new List<SkillIntervalStatistics>();
		private IDictionary<Guid, int> _workloadBacklogDictionary = new Dictionary<Guid, int>();

		public IList<SkillIntervalStatistics> LoadActualCallPerSkillInterval(IList<ISkill> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			return _latestStatisticsTimeAndWorkload
				.OrderBy(o => o.StartTime)
				.ToList();
		}

		public int LoadActualEmailBacklogForWorkload(Guid workloadId, DateTimePeriod closedPeriod)
		{
			if (_workloadBacklogDictionary.ContainsKey(workloadId))
				return _workloadBacklogDictionary[workloadId];
			return 0;
		}

		public void HasStatistics(IList<SkillIntervalStatistics> latestStatisticsTimeAndWorkload)
		{
			_latestStatisticsTimeAndWorkload = latestStatisticsTimeAndWorkload;
		}

		public void HasBacklog(Guid workloadId, int backLog)
		{
			_workloadBacklogDictionary.Add(workloadId, backLog);
		}
	}
}