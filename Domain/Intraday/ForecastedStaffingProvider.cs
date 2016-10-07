using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Calculation;
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

		public ForecastedStaffingModel Load(Guid[] skillIdList, DateTime? latestStatisticsTime, int minutesPerInterval, IList<SkillIntervalStatistics> actualCallsPerSkillInterval)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var staffingIntervals = new List<StaffingIntervalModel>();
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersNowStartOfDayUtc = TimeZoneHelper.ConvertToUtc(usersNow.Date, _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);
			TimeSpan wantedIntervalResolution = TimeSpan.FromMinutes(minutesPerInterval);
			var staffingCalculatorService = new StaffingCalculatorServiceFacade();

			var callsPerSkill= new Dictionary<Guid, List<SkillIntervalStatistics>>();
			var actualStaffingPerSkill= new  List<StaffingIntervalModel>();

			var latestStatisticsTimeUtc = latestStatisticsTime.HasValue
				? TimeZoneHelper.ConvertToUtc(latestStatisticsTime.Value, _timeZone.TimeZone())
				: (DateTime?) null;

			var skills = _skillRepository.LoadSkills(skillIdList);
			foreach (var skill in skills)
			{
				var actualStatsPerInterval = actualCallsPerSkillInterval.Where(s => s.SkillId == skill.Id).ToList();
				var mergedTaskPeriodList = new List<ITemplateTaskPeriod>();
				var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday.AddDays(-1), usersToday.AddDays(1)), new[] { skill }, scenario);
				if (skillDays.Count == 0)
					continue;
				
				foreach (var skillDay in skillDays)
				{
					staffingIntervals.AddRange(getStaffingIntervalModels(skillDay, wantedIntervalResolution));
				    var templateTaskPeriods = getTaskPeriods(skillDay, minutesPerInterval, latestStatisticsTimeUtc, usersNowStartOfDayUtc)
                        .ToList();
				    mergedTaskPeriodList.AddRange(templateTaskPeriods);
                    if (!templateTaskPeriods.Any())
                        continue;
				    var skillOpenTime = TimeZoneHelper.ConvertFromUtc(templateTaskPeriods.Min(t => t.Period.StartDateTime), _timeZone.TimeZone());
				    var skillEndTime = TimeZoneHelper.ConvertFromUtc(templateTaskPeriods.Max(t => t.Period.EndDateTime), _timeZone.TimeZone());
				    actualStatsPerInterval = actualStatsPerInterval
                        .Where(x => x.StartTime >= skillOpenTime && x.StartTime < skillEndTime)
                        .ToList();

                    actualStaffingPerSkill.AddRange(getActualStaffing(actualStatsPerInterval, skillDay, staffingCalculatorService, wantedIntervalResolution));
				}

				callsPerSkill.Add(skill.Id.Value, mergedTaskPeriodList.Select(x => new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = TimeZoneHelper.ConvertFromUtc(x.Period.StartDateTime, _timeZone.TimeZone()),
					Calls = x.TotalTasks
				})
				.ToList());

				
			}
			
			return new ForecastedStaffingModel()
			{
				StaffingIntervals = staffingIntervals,
				CallsPerSkill = callsPerSkill,
				ActualStaffingPerSkill = actualStaffingPerSkill
			};
		}

		private List<StaffingIntervalModel> getActualStaffing(
			List<SkillIntervalStatistics> actualStatsPerInterval, 
			ISkillDay skillDay, 
			StaffingCalculatorServiceFacade staffingCalculatorService, 
			TimeSpan wantedIntervalResolution)
		{
            
            var actualStaffingIntervals = new List<StaffingIntervalModel>();

		    foreach (var actualStatsinterval in actualStatsPerInterval)
		    {
                var actualStatsStartTimeUtc = TimeZoneHelper.ConvertToUtc(actualStatsinterval.StartTime, _timeZone.TimeZone());
                var skillData =
                    skillDay.SkillDataPeriodCollection.SingleOrDefault(
                        skillDataPeriod => skillDataPeriod.Period.StartDateTime <= actualStatsStartTimeUtc &&
                                                 skillDataPeriod.Period.EndDateTime > actualStatsStartTimeUtc
                    );

		        if (skillData == null)
		            continue;

		        actualStaffingIntervals.Add(new StaffingIntervalModel()
		        {
		            SkillId = skillDay.Skill.Id.Value,
		            StartTime = actualStatsinterval.StartTime,
		            Agents = staffingCalculatorService.AgentsUseOccupancy(
		                skillData.ServiceAgreement.ServiceLevel.Percent.Value,
		                (int) skillData.ServiceAgreement.ServiceLevel.Seconds,
		                actualStatsinterval.Calls,
		                actualStatsinterval.AverageHandleTime,
		                wantedIntervalResolution,
		                skillData.ServiceAgreement.MinOccupancy.Value,
		                skillData.ServiceAgreement.MaxOccupancy.Value,
		                skillDay.Skill.MaxParallelTasks
		            )
		        });
		    }
			
			return actualStaffingIntervals;
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

		public List<StaffingIntervalModel> ActualStaffingPerSkill { get; set; }
	}
}
