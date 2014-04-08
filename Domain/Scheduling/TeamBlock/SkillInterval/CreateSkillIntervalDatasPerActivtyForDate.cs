using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ICreateSkillIntervalDatasPerActivtyForDate
	{
		Dictionary<IActivity, IList<ISkillIntervalData>> CreateFor(DateOnly dateOnly, List<ISkill> skills,
																				   ISchedulingResultStateHolder
																					   schedulingResultStateHolder);
	}

	public class CreateSkillIntervalDatasPerActivtyForDate : ICreateSkillIntervalDatasPerActivtyForDate
	{
		private readonly ICalculateAggregatedDataForActivtyAndDate _calculateAggregatedDataForActivtyAndDate;
		private readonly ISkillResolutionProvider _resolutionProvider;

		public CreateSkillIntervalDatasPerActivtyForDate(ICalculateAggregatedDataForActivtyAndDate calculateAggregatedDataForActivtyAndDate, ISkillResolutionProvider resolutionProvider)
		{
			_calculateAggregatedDataForActivtyAndDate = calculateAggregatedDataForActivtyAndDate;
			_resolutionProvider = resolutionProvider;
		}

		public Dictionary<IActivity, IList<ISkillIntervalData>> CreateFor(DateOnly dateOnly, List<ISkill> skills,
		                                                                  ISchedulingResultStateHolder
			                                                                  schedulingResultStateHolder)
		{
			var minimumResolution = _resolutionProvider.MinimumResolution(skills);
			var skilldaysForDate = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {dateOnly});
			var skillDaysForPersonalSkill = new List<ISkillDay>();
			foreach (var skillDay in skilldaysForDate)
			{
				if (skills.Contains(skillDay.Skill))
					skillDaysForPersonalSkill.Add(skillDay);
			}

			var skillActivities = new HashSet<IActivity>();
			foreach (var skillDay in skillDaysForPersonalSkill)
			{
				skillActivities.Add(skillDay.Skill.Activity);
			}
			foreach (var skill in skills)
			{
				skillActivities.Add(skill.Activity);
			}

			var dayIntervalDataPerActivity = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			foreach (var skillActivity in skillActivities)
			{
				var dayIntervalData = _calculateAggregatedDataForActivtyAndDate.CalculateFor(skillDaysForPersonalSkill, skillActivity, minimumResolution);
				dayIntervalDataPerActivity.Add(skillActivity, dayIntervalData);
			}
			return dayIntervalDataPerActivity;
		}
	}
}