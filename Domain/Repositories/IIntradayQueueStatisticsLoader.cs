using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IIntradayQueueStatisticsLoader
	{
		IList<SkillIntervalStatistics> LoadSkillVolumeStatistics(IList<ISkill> skills, DateTime startOfDayUtc);
		IList<SkillIntervalStatistics> LoadActualCallPerSkillInterval(IList<ISkill> skills, TimeZoneInfo timeZone, DateOnly today);
		int LoadActualEmailBacklogForWorkload(Guid workloadId, DateTimePeriod closedPeriod);
	}
}