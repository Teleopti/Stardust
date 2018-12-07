using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IIntradayQueueStatisticsLoader
	{
		IList<SkillIntervalStatistics> LoadSkillVolumeStatistics(IList<ISkill> skills, DateTime startOfDayUtc);
		int LoadActualEmailBacklogForWorkload(Guid workloadId, DateTimePeriod closedPeriod);
	}
}