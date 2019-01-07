using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class SchedulingResultStateHolderExtensions
	{
		public static int SetSkills(this ISchedulingResultStateHolder stateHolder,
			ILoaderDeciderResult deciderResult, IEnumerable<ISkill> allSkills)
		{
			var skills = new HashSet<ISkill>(allSkills);
			var ret = deciderResult.FilterSkills(allSkills, x => skills.Remove(x), x => skills.Add(x));
			stateHolder.Skills = skills;
			return ret;
		}
		
		public static IEnumerable<ISkillDay> SkillDaysOnDateOnly(this ISchedulingResultStateHolder stateHolder, IEnumerable<DateOnly> theDateList)
		{
			return stateHolder.SkillDays == null ? 
				Enumerable.Empty<ISkillDay>() : 
				stateHolder.SkillDays.FilterOnDates(theDateList);
		}

		public static ISkillDay SkillDayOnSkillAndDateOnly(this ISchedulingResultStateHolder stateHolder, ISkill skill, DateOnly dateOnly)
		{
			if (stateHolder.SkillDays != null && stateHolder.SkillDays.TryGetValue(skill, out var foundSkillDays))
			{
				return foundSkillDays.FirstOrDefault(s => s.CurrentDate == dateOnly);
			}

			return null;
		}
	}
}