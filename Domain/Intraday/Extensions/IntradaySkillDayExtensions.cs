
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
				var templateTaskPeriods = GetTemplateTaskPeriods(skillDay, minutesPerInterval, startAtUtc, endAtUtc).ToList();
				skillStatsRange.Add(GetSkillStatsRange(templateTaskPeriods, skillDay));
			}

			return skillStatsRange;
		}

		private static SkillDayStatsRange GetSkillStatsRange(IEnumerable<ITemplateTaskPeriod> templateTaskPeriods,
			ISkillDay skillDay)
		{
			if (!templateTaskPeriods.Any())
				return new SkillDayStatsRange();

			var startTime = new DateTime();
			var endTime = new DateTime();
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				if (workloadDay.OpenTaskPeriodList.Count <= 0)
					continue;

				var start = workloadDay.OpenTaskPeriodList.Min(x => x.Period.StartDateTime);
				var end = workloadDay.OpenTaskPeriodList.Max(x => x.Period.EndDateTime);
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

		private static IEnumerable<ITemplateTaskPeriod> GetTemplateTaskPeriods(ISkillDay skillDay, int minutesPerInterval,
			DateTime startOfPeriodUtc, DateTime endOfPeriodUtc)
		{
			var taskPeriods = new List<ITemplateTaskPeriod>();
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				IList<ITemplateTaskPeriod> templateTaskPeriodCollection = workloadDay.OpenTaskPeriodList;
				if (workloadDay.OpenTaskPeriodList.Any() && minutesPerInterval <= skillDay.Skill.DefaultResolution)
				{
					if (minutesPerInterval < skillDay.Skill.DefaultResolution ||
						IsTaskPeriodsMerged(templateTaskPeriodCollection, skillDay.Skill.DefaultResolution))
						templateTaskPeriodCollection =
							SplitTaskPeriods(templateTaskPeriodCollection, TimeSpan.FromMinutes(minutesPerInterval));

					var temp = templateTaskPeriodCollection.Where(t =>
						t.Period.StartDateTime >= startOfPeriodUtc && t.Period.StartDateTime < endOfPeriodUtc);
					taskPeriods.AddRange(temp);
				}
			}

			return taskPeriods;
		}

		private static bool IsTaskPeriodsMerged(IList<ITemplateTaskPeriod> taskPeriodCollection, int skillResolution)
		{
			var periodStart = taskPeriodCollection.Min(x => x.Period.StartDateTime);
			var periodEnd = taskPeriodCollection.Max(x => x.Period.EndDateTime);
			var periodLength = (int) periodEnd.Subtract(periodStart).TotalMinutes;
			var expectedIntervalCount = periodLength / skillResolution;
			return (expectedIntervalCount != taskPeriodCollection.Count);
		}

		private static IList<ITemplateTaskPeriod> SplitTaskPeriods(IList<ITemplateTaskPeriod> templateTaskPeriodCollection,
			TimeSpan periodLength)
		{
			List<ITemplateTaskPeriod> returnList = new List<ITemplateTaskPeriod>();
			foreach (var taskPeriod in templateTaskPeriodCollection)
			{
				var splittedTaskPeriods = taskPeriod.Split(periodLength);
				returnList.AddRange(splittedTaskPeriods.Select(p => new TemplateTaskPeriod(
					new Task(p.TotalTasks, p.TotalAverageTaskTime, p.TotalAverageAfterTaskTime), p.Period)));
			}

			return returnList;
		}
	}
}
