using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ICreateSkillIntervalDataPerDateAndActivity
	{
		Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo,
																						  ISchedulingResultStateHolder
																							  schedulingResultStateHolder);
	}

	public class CreateSkillIntervalDataPerDateAndActivity : ICreateSkillIntervalDataPerDateAndActivity
	{
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly ICreateSkillIntervalDatasPerActivtyForDate _createSkillIntervalDatasPerActivtyForDate;

		public CreateSkillIntervalDataPerDateAndActivity(IGroupPersonSkillAggregator groupPersonSkillAggregator,
		                                                 ICreateSkillIntervalDatasPerActivtyForDate
			                                                 createSkillIntervalDatasPerActivtyForDate)
		{
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_createSkillIntervalDatasPerActivtyForDate = createSkillIntervalDatasPerActivtyForDate;
		}

		public Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo,
		                                                                                         ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var dayIntervalDataPerDateAndActivity = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers;
			var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
			var skills = _groupPersonSkillAggregator.AggregatedSkills(groupMembers, blockPeriod).ToList();
			

			foreach (var dateOnly in blockPeriod.DayCollection())
			{
				var dayIntervalDataPerActivity = _createSkillIntervalDatasPerActivtyForDate.CreateFor(dateOnly, skills,
				                                                                                      schedulingResultStateHolder);
				dayIntervalDataPerDateAndActivity.Add(dateOnly, dayIntervalDataPerActivity);
			}

			var extraDate = blockPeriod.EndDate.AddDays(1);
			var extraDayIntervalDataPerActivity =
				_createSkillIntervalDatasPerActivtyForDate.CreateFor(blockPeriod.EndDate.AddDays(1), skills,
				                                                     schedulingResultStateHolder);
			dayIntervalDataPerDateAndActivity.Add(extraDate, extraDayIntervalDataPerActivity);
			return dayIntervalDataPerDateAndActivity;
		}
	}
}