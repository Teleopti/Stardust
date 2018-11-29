using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Intraday.Extensions
{
	public static class IntradaySkillDayExtensions
	{
		public static IList<SkillDayStatsRange> GetStatsRanges(
			this IEnumerable<ISkillDay> skillDays,
			DateTime startAtUtc,
			DateTime endAtUtc,
			int minutesPerInterval)
		{
			return skillDays.Select(getSkillStatsRange).ToList();
		}

		private static SkillDayStatsRange getSkillStatsRange(ISkillDay skillDay)
		{
			var timePeriods = skillDay.OpenHours();
			if (timePeriods.IsEmpty()) return new SkillDayStatsRange();

			var start = timePeriods.Min(t => t.StartTime);
			var end = timePeriods.Max(t => t.EndTime);

			return new SkillDayStatsRange
			{
				SkillId = skillDay.Skill.Id.Value,
				SkillDayDate = skillDay.CurrentDate,
				RangePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(skillDay.CurrentDate.Date.Add(start),
					skillDay.CurrentDate.Date.Add(end), skillDay.Skill.TimeZone)
			};
		}
	}
}
