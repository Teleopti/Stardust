using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ICreateSkillIntervalDatasPerActivtyForDate
	{
		//Dictionary<IActivity, IList<ISkillIntervalData>> CreateFor(DateOnly dateOnly, HashSet<ISkill> skills, IEnumerable<ISkillDay> allSkillDays);

		Dictionary<IActivity, IList<ISkillIntervalData>> CreateForAgent(DateOnly dateOnly, HashSet<ISkill> skills,
			IEnumerable<ISkillDay> allSkillDays, TimeZoneInfo agenTimeZoneInfo);
	}

	public class CreateSkillIntervalDatasPerActivtyForDate : ICreateSkillIntervalDatasPerActivtyForDate
	{
		private readonly ICalculateAggregatedDataForActivtyAndDate _calculateAggregatedDataForActivtyAndDate;
		
		public CreateSkillIntervalDatasPerActivtyForDate(ICalculateAggregatedDataForActivtyAndDate calculateAggregatedDataForActivtyAndDate)
		{
			_calculateAggregatedDataForActivtyAndDate = calculateAggregatedDataForActivtyAndDate;
		}

		public Dictionary<IActivity, IList<ISkillIntervalData>> CreateForAgent(DateOnly dateOnly, HashSet<ISkill> skills, IEnumerable<ISkillDay> allSkillDays, TimeZoneInfo agenTimeZoneInfo)
		{
			var minimumResolution = int.MaxValue;
			if (skills.Any())
				minimumResolution = skills.Min(x => x.DefaultResolution);
			var skilldaysForDate = allSkillDays.FilterOnDate(dateOnly);
			var skillDaysForPersonalSkill = skilldaysForDate.Where(s => skills.Contains(s.Skill));

			var skillActivities = skills.Select(s => s.Activity).ToHashSet();

			var skillDaysForPersonalSkillByActivity = skillDaysForPersonalSkill.ToLookup(s => s.Skill.Activity);
			return skillActivities.ToDictionary(a => a,
				skillActivity => _calculateAggregatedDataForActivtyAndDate.CalculateForAgent(skillDaysForPersonalSkillByActivity[skillActivity], minimumResolution, agenTimeZoneInfo));
		}
	}
}