using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
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
		private readonly IMaxSeatSkillAggregator  _maxSeatSkillAggregator;
		private readonly ISkillIntervalDataDivider  _intervalDataDivider;

		public CreateSkillIntervalDataPerDateAndActivity(IGroupPersonSkillAggregator groupPersonSkillAggregator,
		                                                 ICreateSkillIntervalDatasPerActivtyForDate
			                                                 createSkillIntervalDatasPerActivtyForDate, IMaxSeatSkillAggregator maxSeatSkillAggregator, ISkillIntervalDataDivider intervalDataDivider)
		{
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_createSkillIntervalDatasPerActivtyForDate = createSkillIntervalDatasPerActivtyForDate;
			_maxSeatSkillAggregator = maxSeatSkillAggregator;
			_intervalDataDivider = intervalDataDivider;
		}

		public Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> CreateFor(ITeamBlockInfo teamBlockInfo,
		                                                                                         ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var dayIntervalDataPerDateAndActivity = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers;
			var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
			var skills = _groupPersonSkillAggregator.AggregatedSkills(groupMembers, blockPeriod).ToList();
			var maxSeatSkills = _maxSeatSkillAggregator.GetAggregatedSkills(groupMembers.ToList() , blockPeriod);
			bool hasMaxSeatSkill = maxSeatSkills.Any();
			foreach (var dateOnly in blockPeriod.DayCollection())
			{
				var dayIntervalDataPerActivity = _createSkillIntervalDatasPerActivtyForDate.CreateFor(dateOnly, skills,
				                                                                                      schedulingResultStateHolder);
				if (hasMaxSeatSkill)
				{
					foreach (var keyValuePair in dayIntervalDataPerActivity)
					{
						if (keyValuePair.Value.Count == 0) continue;
						var splitedInterval = _intervalDataDivider.SplitSkillIntervalData(keyValuePair.Value.ToList(), 15);
						keyValuePair.Value.Clear();
						foreach (var eachSplitedInterval in splitedInterval)
						{
							keyValuePair.Value.Add(eachSplitedInterval );
						}
					}				
				} 
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