using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedCallsProvider
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;

		public ForecastedCallsProvider(
			INow now,
			IUserTimeZone timeZone
			)
		{
			_now = now;
			_timeZone = timeZone;
		}

		public ForecastedCallsModel Load(IList<ISkill> skills, ICollection<ISkillDay> skillDays, DateTime? latestStatisticsTime, int minutesPerInterval)
		{
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersNowStartOfDayUtc = TimeZoneHelper.ConvertToUtc(usersNow.Date, _timeZone.TimeZone());
			var callsPerSkill= new Dictionary<Guid, List<SkillIntervalStatistics>>();
			var skillStatsRange = new List<SkillDayStatsRange>();
			var latestStatisticsTimeUtc = latestStatisticsTime.HasValue
				? TimeZoneHelper.ConvertToUtc(latestStatisticsTime.Value, _timeZone.TimeZone())
				: (DateTime?) null;

			foreach (var skill in skills)
			{
				var mergedTaskPeriodList = new List<ITemplateTaskPeriod>();
				var skillDaysForSkill = skillDays.Where(x => x.Skill.Id.Value == skill.Id.Value).ToList();
				if (!skillDaysForSkill.Any())
					continue;
								
				foreach (var skillDay in skillDaysForSkill)
				{
				    var templateTaskPeriods = getTaskPeriods(skillDay, minutesPerInterval, latestStatisticsTimeUtc, usersNowStartOfDayUtc)
                        .ToList();
				    mergedTaskPeriodList.AddRange(templateTaskPeriods);

					if (!templateTaskPeriods.Any())
						continue;
					skillStatsRange.Add(new SkillDayStatsRange()
					{
						skillId = skill.Id.Value,
						skillDayId = skillDay.Id.Value,
						RangePeriod = new DateTimePeriod(
							templateTaskPeriods.Min(t => t.Period.StartDateTime), 
							templateTaskPeriods.Max(t => t.Period.EndDateTime))
					});
				}

				var mergedTaskPeriodPerSkill = mergedTaskPeriodList
					.Select(x => new SkillIntervalStatistics
					{
						SkillId = skill.Id.Value,
						StartTime = TimeZoneHelper.ConvertFromUtc(x.Period.StartDateTime, _timeZone.TimeZone()),
						Calls = x.TotalTasks
					})
					.ToList();

				callsPerSkill.Add(skill.Id.Value, mergedTaskPeriodPerSkill
					.GroupBy(h => h.StartTime)
					.Select(s => new SkillIntervalStatistics
					{
						SkillId = skill.Id.Value,
						StartTime = s.First().StartTime,
						Calls = s.Sum(c => c.Calls)
					})
					.OrderBy(g => g.StartTime)
					.ToList());
			}
			
			return new ForecastedCallsModel()
			{
				CallsPerSkill = callsPerSkill,
				SkillDayStatsRange = skillStatsRange
			};
		}

		private IEnumerable<ITemplateTaskPeriod> getTaskPeriods(ISkillDay skillDay, int minutesPerInterval, DateTime? latestStatisticsTimeUtc, DateTime usersNowStartOfDayUtc)
		{
			var taskPeriods = new List<ITemplateTaskPeriod>();
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				taskPeriods.AddRange(taskPeriodsUpUntilNow(workloadDay.TaskPeriodList, minutesPerInterval, skillDay.Skill.DefaultResolution, latestStatisticsTimeUtc, usersNowStartOfDayUtc));
			}
			return taskPeriods;
		}
		

		private IEnumerable<ITemplateTaskPeriod> taskPeriodsUpUntilNow(
			IList<ITemplateTaskPeriod> templateTaskPeriodCollection, 
			int targetMinutesPerInterval, 
			int skillMinutesPerInterval, 
			DateTime? latestStatisticsTimeUtc,
			DateTime? usersNowStartOfDayUtc)
		{
			var returnList = new List<ITemplateTaskPeriod>();
			var periodLength = TimeSpan.FromMinutes(targetMinutesPerInterval);

			if (!latestStatisticsTimeUtc.HasValue)
				return returnList;

			if (targetMinutesPerInterval > skillMinutesPerInterval)
				return returnList;

			if (targetMinutesPerInterval < skillMinutesPerInterval)
			{
				foreach (var taskPeriod in templateTaskPeriodCollection)
				{
					var splittedTaskPeriods = taskPeriod.Split(periodLength);
					returnList.AddRange(splittedTaskPeriods.Select(p => new TemplateTaskPeriod(
						new Task(p.Tasks, p.TotalAverageTaskTime, p.TotalAverageAfterTaskTime), p.Period)));
				}
				return returnList
					.Where(t =>
						t.Period.StartDateTime >= usersNowStartOfDayUtc.Value &&
						t.Period.EndDateTime <= latestStatisticsTimeUtc.Value.AddMinutes(targetMinutesPerInterval)
					)
                    .ToList();
			}

			return templateTaskPeriodCollection
				.Where(t => 
					t.Period.StartDateTime >= usersNowStartOfDayUtc.Value && 
					t.Period.EndDateTime <= latestStatisticsTimeUtc.Value.AddMinutes(targetMinutesPerInterval)
				)
                .ToList();
		}
	}

	public class ForecastedCallsModel
	{
		public Dictionary<Guid, List<SkillIntervalStatistics>> CallsPerSkill { get; set; }
		public IList<SkillDayStatsRange> SkillDayStatsRange { get; set; }
	}

	public class SkillDayStatsRange
	{
		public Guid skillId { get; set; }
		public Guid skillDayId { get; set; }
		public DateTimePeriod RangePeriod { get; set; }

	}
}
