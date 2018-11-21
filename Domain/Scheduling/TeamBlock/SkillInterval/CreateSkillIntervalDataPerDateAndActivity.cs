using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

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
			var dayIntervalDataPerDateAndActivity = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToList();
			var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
			var skills = groupPersonSkillAggregator.AggregatedSkills(groupMembers, blockPeriod).ToList();
			foreach (var dateOnly in blockPeriod.Inflate(1).DayCollection())
			{
				var dayIntervalDataPerActivity = _createSkillIntervalDatasPerActivtyForDate.CreateFor(dateOnly, skills, allSkillDays);
				dayIntervalDataPerDateAndActivity.Add(dateOnly, dayIntervalDataPerActivity);
			}
			return dayIntervalDataPerDateAndActivity;
		}

		public Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> allSkillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator, DateOnlyPeriod period)
		{
			var dayIntervalDataPerDateAndActivity = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToList();
			var skills = groupPersonSkillAggregator.AggregatedSkills(groupMembers, period).ToList();
			foreach (var dateOnly in period.DayCollection())
			{
				var dayIntervalDataPerActivity = _createSkillIntervalDatasPerActivtyForDate.CreateFor(dateOnly, skills, allSkillDays);
				dayIntervalDataPerDateAndActivity.Add(dateOnly, dayIntervalDataPerActivity);
			}
			return dayIntervalDataPerDateAndActivity;
		}
	}
}