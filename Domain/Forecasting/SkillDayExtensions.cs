using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

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
	}
}