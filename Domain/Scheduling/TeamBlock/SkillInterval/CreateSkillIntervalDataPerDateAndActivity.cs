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

		public Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> allSkillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToList();
			var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
			var skills = groupPersonSkillAggregator.AggregatedSkills(groupMembers, blockPeriod).ToList();
			return blockPeriod.Inflate(1).DayCollection().ToDictionary(d => d,
				dateOnly => (IDictionary<IActivity, IList<ISkillIntervalData>>)_createSkillIntervalDatasPerActivtyForDate.CreateFor(dateOnly, skills, allSkillDays));
		}

		public Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> allSkillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator, DateOnlyPeriod period)
		{
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToList();
			var skills = groupPersonSkillAggregator.AggregatedSkills(groupMembers, period).ToList();

			return period.DayCollection().ToDictionary(d => d,
				dateOnly =>
					(IDictionary<IActivity, IList<ISkillIntervalData>>) _createSkillIntervalDatasPerActivtyForDate
						.CreateFor(dateOnly, skills, allSkillDays));
		}
	}
}