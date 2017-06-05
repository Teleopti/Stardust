using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IIntradayQueueStatisticsLoader
	{
		IList<SkillIntervalStatistics> LoadActualCallPerSkillInterval(IList<ISkill> skills, TimeZoneInfo timeZone, DateOnly today);
		int LoadActualEmailBacklogForWorkload(Guid workloadId, TimeZoneInfo timeZone, DateTimePeriod closedPeriod);
	}
}