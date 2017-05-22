using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedCallsProvider
	{
		private readonly IUserTimeZone _timeZone;
		private readonly TaskPeriodsProvider _taskPeriodsProvider;

		public ForecastedCallsProvider(
			IUserTimeZone timeZone,
			TaskPeriodsProvider taskPeriodsProvider
			)
		{
			_timeZone = timeZone;
			_taskPeriodsProvider = taskPeriodsProvider;
		}

		public ForecastedCallsModel Load(IList<ISkill> skills, ICollection<ISkillDay> skillDays, DateTime? latestStatisticsTime, int minutesPerInterval)
		{

			var callsPerSkill= new Dictionary<Guid, List<SkillIntervalStatistics>>();
			var skillStatsRange = new List<SkillDayStatsRange>();

			foreach (var skill in skills)
			{
				var mergedTaskPeriodList = new List<ITemplateTaskPeriod>();
				var skillDaysForSkill = skillDays.Where(x => x.Skill.Id.Value == skill.Id.Value).ToList();
				if (!skillDaysForSkill.Any())
					continue;
								
				foreach (var skillDay in skillDaysForSkill)
				{
				    var templateTaskPeriods = _taskPeriodsProvider.Load(skillDay, minutesPerInterval, latestStatisticsTime)
                        .ToList();
				    mergedTaskPeriodList.AddRange(templateTaskPeriods);

					skillStatsRange.Add(getSkillStatsRange(templateTaskPeriods, skill, skillDay));
				}

				callsPerSkill.Add(skill.Id.Value, getForecastedCalls(mergedTaskPeriodList, skill));
			}
			
			return new ForecastedCallsModel
			{
				CallsPerSkill = callsPerSkill,
				SkillDayStatsRange = skillStatsRange
			};
		}

		private List<SkillIntervalStatistics> getForecastedCalls(List<ITemplateTaskPeriod> mergedTaskPeriodList, ISkill skill)
		{
			var mergedTaskPeriodPerSkill = mergedTaskPeriodList
				.Select(x => new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = TimeZoneHelper.ConvertFromUtc(x.Period.StartDateTime, _timeZone.TimeZone()),
					Calls = x.TotalTasks,
					AverageHandleTime = x.AverageTaskTime.TotalSeconds + x.AverageAfterTaskTime.TotalSeconds
				})
				.ToList();

			var forecastedCalls =  mergedTaskPeriodPerSkill
				.GroupBy(h => h.StartTime)
				.Select(s => new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = s.First().StartTime,
					Calls = s.Sum(c => c.Calls),
					AverageHandleTime = s.Sum(a => a.AverageHandleTime)
				})
				.OrderBy(g => g.StartTime)
				.ToList();

			return forecastedCalls;
		}

		private static SkillDayStatsRange getSkillStatsRange(List<ITemplateTaskPeriod> templateTaskPeriods, 
			ISkill skill, 
			ISkillDay skillDay)
		{
			if (!templateTaskPeriods.Any())
				return new SkillDayStatsRange();

			return new SkillDayStatsRange()
			{
				SkillId = skill.Id.Value,
				SkillDayDate = skillDay.CurrentDate,
				RangePeriod = new DateTimePeriod(
					templateTaskPeriods.Min(t => t.Period.StartDateTime),
					templateTaskPeriods.Max(t => t.Period.EndDateTime))
			};
		}
	}
}
