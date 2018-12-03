using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ICreateSkillIntervalDatasPerActivtyForDate
	{
		Dictionary<IActivity, IList<ISkillIntervalData>> CreateFor(DateOnly dateOnly, List<ISkill> skills, IEnumerable<ISkillDay> allSkillDays);
	}

	public class CreateSkillIntervalDatasPerActivtyForDate : ICreateSkillIntervalDatasPerActivtyForDate
	{
		private readonly ICalculateAggregatedDataForActivtyAndDate _calculateAggregatedDataForActivtyAndDate;
		
		public CreateSkillIntervalDatasPerActivtyForDate(ICalculateAggregatedDataForActivtyAndDate calculateAggregatedDataForActivtyAndDate)
		{
			_calculateAggregatedDataForActivtyAndDate = calculateAggregatedDataForActivtyAndDate;
		}

		public Dictionary<IActivity, IList<ISkillIntervalData>> CreateFor(DateOnly dateOnly, List<ISkill> skills, IEnumerable<ISkillDay> allSkillDays)
		{
			var minimumResolution = int.MaxValue;
			if (skills.Any())
				minimumResolution = skills.Min(x => x.DefaultResolution);
			var skilldaysForDate = allSkillDays.FilterOnDate(dateOnly);
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