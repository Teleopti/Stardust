using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public static class SkillDayExtensions
	{
		public static IEnumerable<ISkillDay> FilterOnDates(this IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, HashSet<DateOnly> dates)
		{
			return skillDays.ToSkillDayEnumerable().Where(k => dates.Contains(k.CurrentDate)).ToArray();
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
			return allSkillDays.GroupBy(k => k.Skill).ToDictionary(k => k.Key, v => (IEnumerable<ISkillDay>)v.ToList());
		}
	}
}