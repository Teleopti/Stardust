using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public class CreateSkillIntervalDataPerDateAndActivity
	{
		private readonly ICreateSkillIntervalDatasPerActivtyForDate _createSkillIntervalDatasPerActivtyForDate;

		public CreateSkillIntervalDataPerDateAndActivity(ICreateSkillIntervalDatasPerActivtyForDate createSkillIntervalDatasPerActivtyForDate)
		{
			_createSkillIntervalDatasPerActivtyForDate = createSkillIntervalDatasPerActivtyForDate;
		}

		public Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateForAgent(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> allSkillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator, TimeZoneInfo agentTimeZoneInfo)
		{
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToArray();
			var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
			var skills = groupPersonSkillAggregator.AggregatedSkills(groupMembers, blockPeriod).ToHashSet();
			return blockPeriod.Inflate(1).DayCollection().ToDictionary(d => d,
				dateOnly => (IDictionary<IActivity, IList<ISkillIntervalData>>)_createSkillIntervalDatasPerActivtyForDate.CreateForAgent(dateOnly, skills, allSkillDays, agentTimeZoneInfo));
		}

		public Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> allSkillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToArray();
			var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
			var skills = groupPersonSkillAggregator.AggregatedSkills(groupMembers, blockPeriod).ToHashSet();
			return blockPeriod.Inflate(1).DayCollection().ToDictionary(d => d,
				dateOnly => (IDictionary<IActivity, IList<ISkillIntervalData>>)_createSkillIntervalDatasPerActivtyForDate.CreateFor(dateOnly, skills, allSkillDays));
		}

		public Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> allSkillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator, DateOnlyPeriod period)
		{
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToList();
			var skills = groupPersonSkillAggregator.AggregatedSkills(groupMembers, period).ToHashSet();

			return period.DayCollection().ToDictionary(d => d,
				dateOnly =>
					(IDictionary<IActivity, IList<ISkillIntervalData>>) _createSkillIntervalDatasPerActivtyForDate
						.CreateFor(dateOnly, skills, allSkillDays));
		}
	}
}