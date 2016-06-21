using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public static class SkillDayExtensions
	{
		public static IEnumerable<ISkillDay> FilterOnDates(this IEnumerable<ISkillDay> skillDays, IEnumerable<DateOnly> dates)
		{
			var ret = new List<ISkillDay>();
			var days = skillDays.ToLookup(k => k.CurrentDate);
			foreach (var dateOnly in dates)
			{
				ret.AddRange(days[dateOnly]);
			}
			return ret;
		}
	}
}