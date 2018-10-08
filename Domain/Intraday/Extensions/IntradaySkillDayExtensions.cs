using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Interfaces.Domain;

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
			var skillStatsRange = new List<SkillDayStatsRange>();
			foreach (var skillDay in skillDays)
			{
				var templateTaskPeriods = getTemplateTaskPeriods(skillDay, minutesPerInterval, startAtUtc, endAtUtc).ToList();
				skillStatsRange.Add(getSkillStatsRange(templateTaskPeriods, skillDay));
			}

			return skillStatsRange;
		}

		private static SkillDayStatsRange getSkillStatsRange(IEnumerable<ISkillStaffPeriodView> templateTaskPeriods,
			ISkillDay skillDay)
		{
			if (!templateTaskPeriods.Any())
				return new SkillDayStatsRange();

			var startTime = new DateTime();
			var endTime = new DateTime();
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				var openTaskPeriodList = workloadDay.OpenTaskPeriodList;
				if (openTaskPeriodList.Count <= 0)
					continue;

				var start = openTaskPeriodList.Min(x => x.Period.StartDateTime);
				var end = openTaskPeriodList.Max(x => x.Period.EndDateTime);
				if (startTime != null || startTime > start)
					startTime = start;
				if (end != null || endTime < end)
					endTime = end;
			}

			return new SkillDayStatsRange()
			{
				SkillId = skillDay.Skill.Id.Value,
				SkillDayDate = skillDay.CurrentDate,
				RangePeriod = new DateTimePeriod(startTime, endTime)
			};
		}

		private static IEnumerable<ISkillStaffPeriodView> getTemplateTaskPeriods(ISkillDay skillDay, int minutesPerInterval,
			DateTime startOfPeriodUtc, DateTime endOfPeriodUtc)
		{
			return skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval)).Where(t =>
						t.Period.StartDateTime >= startOfPeriodUtc && t.Period.StartDateTime < endOfPeriodUtc);
		}
	}
}
