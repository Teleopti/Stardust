using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingProvider
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;

		public ForecastedStaffingProvider(
			ISkillRepository skillRepository,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			INow now,
			IUserTimeZone timeZone
			)
		{
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_now = now;
			_timeZone = timeZone;
		}

		public ForecastedStaffingModel Load(Guid[] skillIdList, DateTime? latestStatisticsTime, int minutesPerInterval)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var staffingIntervals = new List<StaffingIntervalModel>();
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);
			TimeSpan wantedIntervalResolution = TimeSpan.FromMinutes(minutesPerInterval);

			var workloadInSecondsPerSkill = new Dictionary<Guid, List<SkillWorkload>>();

			var latestStatisticsTimeUtc = latestStatisticsTime.HasValue
				? TimeZoneHelper.ConvertToUtc(latestStatisticsTime.Value, _timeZone.TimeZone())
				: (DateTime?) null;

			var skills = _skillRepository.LoadSkills(skillIdList);
			foreach (var skill in skills)
			{
				var mergedTaskPeriodList = new List<ITemplateTaskPeriod>();
				var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday.AddDays(-1), usersToday.AddDays(1)), new[] { skill }, scenario);
				if (skillDays.Count == 0)
					continue;
				
				foreach (var skillDay in skillDays)
				{
					staffingIntervals.AddRange(getStaffingIntervalModels(skillDay, wantedIntervalResolution));
					mergedTaskPeriodList.AddRange(getTaskPeriods(skillDay, minutesPerInterval, latestStatisticsTimeUtc, _now.UtcDateTime()));
				}

				workloadInSecondsPerSkill.Add(skill.Id.Value, mergedTaskPeriodList.Select(x => new SkillWorkload
				{
					SkillId = skill.Id.Value,
					StartTime = TimeZoneHelper.ConvertFromUtc(x.Period.StartDateTime, _timeZone.TimeZone()),
					WorkloadInSeconds = x.TotalTasks * (x.TotalAverageTaskTime.TotalSeconds + x.TotalAverageAfterTaskTime.TotalSeconds)
				})
				.ToList());
			}
			
			return new ForecastedStaffingModel()
			{
				StaffingIntervals = staffingIntervals,
				WorkloadInSecondsPerSkill = workloadInSecondsPerSkill
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
						t.Period.StartDateTime >= usersNowUtc.Value.Date &&
						t.Period.EndDateTime <= latestStatisticsTimeUtc.Value.AddMinutes(targetMinutesPerInterval)
					);
			}

			return templateTaskPeriodCollection
				.Where(t => 
					t.Period.StartDateTime >= usersNowUtc.Value.Date && 
					t.Period.EndDateTime <= latestStatisticsTimeUtc.Value.AddMinutes(targetMinutesPerInterval)
				);
		}
	}

	public class ForecastedStaffingModel
	{
		public IList<StaffingIntervalModel> StaffingIntervals { get; set; }
		public Dictionary<Guid, List<SkillWorkload>> WorkloadInSecondsPerSkill { get; set; }
	}
}
