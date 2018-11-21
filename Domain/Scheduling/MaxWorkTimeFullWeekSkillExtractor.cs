using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IMaxWorkTimeFullWeekSkillExtractor
	{
		Dictionary<DateOnly, TimeSpan> Extract(IScheduleMatrixPro scheduleMatrixPro, ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator);
	}

	public class MaxWorkTimeFullWeekSkillExtractor : IMaxWorkTimeFullWeekSkillExtractor
	{
		public Dictionary<DateOnly, TimeSpan> Extract(IScheduleMatrixPro scheduleMatrixPro, ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			throw new NotImplementedException();
		}
	}

	public class MaxWorkTimeFullWeekSkillExtractorDoNothing : IMaxWorkTimeFullWeekSkillExtractor
	{
		public Dictionary<DateOnly, TimeSpan> Extract(IScheduleMatrixPro scheduleMatrixPro, ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			throw new NotImplementedException();
		}
	}
}
