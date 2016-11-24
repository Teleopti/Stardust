using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingProvider
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;

		public ForecastedStaffingProvider(
			INow now,
			IUserTimeZone timeZone
			)
		{
			_now = now;
			_timeZone = timeZone;
		}

		public ForecastedStaffingModel Load(IList<ISkill> skills, ICollection<ISkillDay> skillDays, DateTime? latestStatisticsTime, int minutesPerInterval)
		{
			var staffingIntervals = new List<StaffingIntervalModel>();
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersNowStartOfDayUtc = TimeZoneHelper.ConvertToUtc(usersNow.Date, _timeZone.TimeZone());
			TimeSpan wantedIntervalResolution = TimeSpan.FromMinutes(minutesPerInterval);

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
					staffingIntervals.AddRange(getStaffingIntervalModels(skillDay, wantedIntervalResolution));
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
			
			return new ForecastedStaffingModel()
			{
				StaffingIntervals = staffingIntervals,
				CallsPerSkill = callsPerSkill,
				SkillDayStatsRange = skillStatsRange
			};
		}

		private IEnumerable<ITemplateTaskPeriod> getTaskPeriods(ISkillDay skillDay, int minutesPerInterval, DateTime? latestStatisticsTimeUtc, DateTime utcDateTime)
		{
			var taskPeriods = new List<ITemplateTaskPeriod>();
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				taskPeriods.AddRange(taskPeriodsUpUntilNow(workloadDay.TaskPeriodList, minutesPerInterval, skillDay.Skill.DefaultResolution, latestStatisticsTimeUtc, utcDateTime));
			}
			return taskPeriods;
		}

		private IEnumerable<StaffingIntervalModel> getStaffingIntervalModels(ISkillDay skillDay, TimeSpan wantedIntervalResolution)
		{
			var skillStaffPeriods = skillDay.SkillStaffPeriodViewCollection(wantedIntervalResolution);

			return skillStaffPeriods.Select(skillStaffPeriod => new StaffingIntervalModel
			{
				SkillId = skillDay.Skill.Id.Value,
				StartTime = TimeZoneHelper.ConvertFromUtc(skillStaffPeriod.Period.StartDateTime, _timeZone.TimeZone()),
				Agents = skillStaffPeriod.FStaff
			});
		}

		private IEnumerable<ITemplateTaskPeriod> taskPeriodsUpUntilNow(
			IList<ITemplateTaskPeriod> templateTaskPeriodCollection, 
			int targetMinutesPerInterval, 
			int skillMinutesPerInterval, 
			DateTime? latestStatisticsTimeUtc,
			DateTime? usersNowUtc)
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
						t.Period.StartDateTime >= usersNowUtc.Value &&
						t.Period.EndDateTime <= latestStatisticsTimeUtc.Value.AddMinutes(targetMinutesPerInterval)
					)
                    .ToList();
			}

			return templateTaskPeriodCollection
				.Where(t => 
					t.Period.StartDateTime >= usersNowUtc.Value && 
					t.Period.EndDateTime <= latestStatisticsTimeUtc.Value.AddMinutes(targetMinutesPerInterval)
				)
                .ToList();
		}
	}

	public class ForecastedStaffingModel
	{
		public IList<StaffingIntervalModel> StaffingIntervals { get; set; }
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
