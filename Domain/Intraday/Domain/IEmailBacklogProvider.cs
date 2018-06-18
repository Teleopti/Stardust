using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public interface IEmailBacklogProvider
	{
		Dictionary<Guid, int> GetStatisticsBacklogByWorkload(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, IList<SkillIntervalStatistics> actualVolumePerWorkloadInterval, DateOnly userDateOnly, int minutesPerInterval, IList<SkillDayStatsRange> skillDayStatsRange);
	}
}