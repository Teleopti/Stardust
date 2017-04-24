using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ICreateSkillIntervalDataPerDateAndActivity
	{
		Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> allSkillDays, IGroupPersonSkillAggregator groupPersonSkillAggregator);
	}

	public class CreateSkillIntervalDataPerDateAndActivity : ICreateSkillIntervalDataPerDateAndActivity
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
			foreach (var dateOnly in blockPeriod.DayCollection())
			{
				var dayIntervalDataPerActivity = _createSkillIntervalDatasPerActivtyForDate.CreateFor(dateOnly, skills, allSkillDays);
				dayIntervalDataPerDateAndActivity.Add(dateOnly, dayIntervalDataPerActivity);
			}

			var extraDate = blockPeriod.EndDate.AddDays(1);
			var extraDayIntervalDataPerActivity = _createSkillIntervalDatasPerActivtyForDate.CreateFor(blockPeriod.EndDate.AddDays(1), skills, allSkillDays);

			dayIntervalDataPerDateAndActivity.Add(extraDate, extraDayIntervalDataPerActivity);
			return dayIntervalDataPerDateAndActivity;
		}
	}
}