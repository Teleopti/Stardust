using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public static class SkillDayExtensions
	{
		public static IEnumerable<ISkillDay> FilterOnDates(this IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, IEnumerable<DateOnly> dates)
		{
			var ret = new List<ISkillDay>();
			var days = skillDays.SelectMany(s => s.Value).ToLookup(k => k.CurrentDate);
			foreach (var dateOnly in dates)
			{
				ret.AddRange(days[dateOnly]);
			}
			return ret;
		}

		public static IEnumerable<ISkillDay> FilterOnDate(this IEnumerable<ISkillDay> allSkillDays, DateOnly date)
		{
			return allSkillDays.Where(x => x.CurrentDate == date);
		}

		public static IEnumerable<ISkillDay> ToSkillDayEnumerable(this IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays)
		{
			return skillDays.Values.SelectMany(s => s);
		}

		public static IDictionary<ISkill, IEnumerable<ISkillDay>> ToSkillSkillDayDictionary(this IEnumerable<ISkillDay> allSkillDays)
		{
			var ret = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			foreach (var skillDay in allSkillDays)
			{
				if (ret.TryGetValue(skillDay.Skill, out var skillDaysForSkill))
				{
					((ICollection<ISkillDay>)skillDaysForSkill).Add(skillDay);
				}
				else
				{
					ret.Add(skillDay.Skill, new List<ISkillDay> {skillDay});
				}
			}
			return ret;
		}
	}
}