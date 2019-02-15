using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public interface IIntradayStaffingService
	{
		IList<SkillStaffingIntervalLightModel> GetScheduledStaffing(
			Guid[] skillIdList, 
			DateTime fromUtc, 
			DateTime toUtc,
			TimeSpan resolution, 
			bool useShrinkage);

		IList<StaffingInterval> GetRequiredStaffing(
			IList<SkillIntervalStatistics> actualStatistics,
			IList<ISkill> skills,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays,
			IList<StaffingInterval> forecastedStaffingIntervals,
			TimeSpan resolution, IList<SkillDayStatsRange> skillDayStatsRange,
			Dictionary<Guid, int> workloadBacklog);
	}

	public class IntradayStaffingService : IIntradayStaffingService
	{
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IntradayStatisticsService _intradayStatisticsService;
		private readonly ILoadSkillDaysWithPeriodFlexibility _loadSkillDaysWithPeriodFlexibility;

		public IntradayStaffingService(
			ICurrentScenario currentScenario,
			IResourceCalculation resourceCalculation,
			IntradayStatisticsService intradayStatisticsService,
			ISkillCombinationResourceRepository skillCombinationResourceRepository, 
			ISkillRepository skillRepository,
			ILoadSkillDaysWithPeriodFlexibility loadSkillDaysWithPeriodFlexibility)
		{
			_skillCombinationResourceRepository = skillCombinationResourceRepository ?? throw new ArgumentNullException(nameof(skillCombinationResourceRepository));
			_skillRepository = skillRepository ?? throw new ArgumentNullException(nameof(skillRepository));
			_loadSkillDaysWithPeriodFlexibility = loadSkillDaysWithPeriodFlexibility;
			_currentScenario = currentScenario ?? throw new ArgumentNullException(nameof(currentScenario));
			_resourceCalculation = resourceCalculation ?? throw new ArgumentNullException(nameof(resourceCalculation));
			_intradayStatisticsService = intradayStatisticsService ?? throw new ArgumentNullException(nameof(intradayStatisticsService));
		}

		public IList<SkillStaffingIntervalLightModel> GetScheduledStaffing(Guid[] skillIdList, DateTime fromUtc, DateTime toUtc, TimeSpan resolution, bool useShrinkage)
		{
			var period = new DateTimePeriod(fromUtc, toUtc);
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			if (!combinationResources.Any()) return new List<SkillStaffingIntervalLightModel>();

			var skills = _skillRepository.LoadAll().ToList();

			var firstPeriodDateInSkillTimeZone = new DateOnly(skills.Select(x => TimeZoneInfo.ConvertTimeFromUtc(fromUtc, x.TimeZone)).Min());
			var lastPeriodDateInSkillTimeZone = new DateOnly(skills.Select(x => TimeZoneInfo.ConvertTimeFromUtc(toUtc, x.TimeZone)).Max());
			var dateOnlyPeriod = new DateOnlyPeriod(firstPeriodDateInSkillTimeZone, lastPeriodDateInSkillTimeZone);

			var skillDays = _loadSkillDaysWithPeriodFlexibility.Load(dateOnlyPeriod, skills, _currentScenario.Current()).Values.SelectMany(i => i).ToList();
			var returnList = new HashSet<SkillStaffingInterval>();
			foreach (var skillDay in skillDays)
			{
				skillDay.RecalculateDailyTasks();

				if (useShrinkage)
				{
					foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
					{
						skillStaffPeriod.Payload.UseShrinkage = true;
					}
				}

				var skillStaffPeriods = skillDay.SkillStaffPeriodCollection;

				var intervals = skillStaffPeriods.Select(skillStaffPeriod => new SkillStaffingInterval
				{
					SkillId = skillDay.Skill.Id.GetValueOrDefault(),
					StartDateTime = skillStaffPeriod.Period.StartDateTime,
					EndDateTime = skillStaffPeriod.Period.EndDateTime,
					Forecast = skillStaffPeriod.FStaff,
					StaffingLevel = 0,
				});
				intervals.ForEach(i => returnList.Add(i));
			}
			var skillStaffingIntervals = returnList
				.Where(x => period.Contains(x.StartDateTime) || x.DateTimePeriod.Contains(period.StartDateTime));
			skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var relevantSkillStaffPeriods = skillStaffingIntervals
				.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
					.ToDictionary(k => k.Key,
						v =>
							(IResourceCalculationPeriodDictionary)
							new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
								s => (IResourceCalculationPeriod)s)));
			var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));

			using (new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, false))))
			{
				_resourceCalculation
					.ResourceCalculate(dateOnlyPeriod, resCalcData, () => new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, true))));
			}

			var allSkillStaffingIntervals = skillStaffingIntervals;
			var relevantSkillStaffingIntervals = allSkillStaffingIntervals.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime && skillIdList.Contains(x.SkillId)).ToList();
			var splittedIntervals = split(relevantSkillStaffingIntervals, resolution, false);

			return splittedIntervals.GroupBy(x => new
				{
					x.Id,
					StartDateTime = x.StartDateTime
				})
				.Select(x => new SkillStaffingIntervalLightModel
				{
					Id = x.Key.Id,
					StartDateTime = x.Key.StartDateTime,
					EndDateTime = x.Key.StartDateTime.AddMinutes(resolution.TotalMinutes),
					StaffingLevel = x.Sum(y => y.StaffingLevel)
				})
				.OrderBy(o => o.StartDateTime)
				.ToList();
		}

		public IList<StaffingInterval> GetRequiredStaffing(
			IList<SkillIntervalStatistics> actualStatistics,
			IList<ISkill> skills,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays,
			IList<StaffingInterval> forecastedStaffingIntervals,
			TimeSpan resolution, IList<SkillDayStatsRange> skillDayStatsRange,
			Dictionary<Guid, int> workloadBacklog)
		{

			var actualStaffingIntervals = new List<StaffingInterval>();
			if (actualStatistics == null || !actualStatistics.Any())
				return actualStaffingIntervals;

			var skillDaysBySkill = skillDays
				.SelectMany(x => x.Value)
				.Select(s => new { Original = s, Clone = s.NoneEntityClone() })
				.ToLookup(s => s.Original.Skill.Id.Value);

			var actualStatisticsBySkill = actualStatistics.ToLookup(s => s.SkillId);
			foreach (var skill in skills)
			{
				var actualStatsPerSkillInterval = actualStatisticsBySkill[skill.Id.Value].ToList();
				var skillDaysForSkill = skillDaysBySkill[skill.Id.Value].ToArray();
				new SkillDayCalculator(skill, skillDaysForSkill.Select(s => s.Clone), new DateOnlyPeriod());

				foreach (var skillDay in skillDaysForSkill)
				{
					var skillDayStats = _intradayStatisticsService.GetSkillDayStatistics(skillDayStatsRange, skill, skillDay.Original.CurrentDate, actualStatsPerSkillInterval, resolution);

					mapWorkloadIds(skillDay.Clone, skillDay.Original);
					assignActualWorkloadToClonedSkillDay(skillDay.Clone, skillDayStats, workloadBacklog);
					actualStaffingIntervals.AddRange(GetRequiredStaffing(resolution, skillDayStats, skillDay.Clone));
				}
				foreach (var skillDayToReset in skillDaysForSkill)
				{
					reset(skillDayToReset.Clone, skillDayToReset.Original);
				}
			}

			return actualStaffingIntervals;
		}

		private static IList<SkillStaffingIntervalLightModel> split(List<SkillStaffingInterval> staffingList, TimeSpan resolution, bool useShrinkage)
		{
			var dividedIntervals = new List<SkillStaffingIntervalLightModel>();

			foreach (var skillStaffingInterval in staffingList)
			{
				if (!skillStaffingInterval.GetTimeSpan().Equals(resolution))
				{
					var startInterval = skillStaffingInterval.StartDateTime;
					while (startInterval < skillStaffingInterval.EndDateTime)
					{
						if (useShrinkage)
							dividedIntervals.Add(new SkillStaffingIntervalLightModel
							{
								Id = skillStaffingInterval.SkillId,
								StartDateTime = startInterval,
								EndDateTime = startInterval.Add(resolution),
								StaffingLevel = skillStaffingInterval.StaffingLevelWithShrinkage
							});
						else
							dividedIntervals.Add(new SkillStaffingIntervalLightModel
							{
								Id = skillStaffingInterval.SkillId,
								StartDateTime = startInterval,
								EndDateTime = startInterval.Add(resolution),
								StaffingLevel = skillStaffingInterval.StaffingLevel
							});
						startInterval = startInterval.Add(resolution);
					}
				}
				else
				{
					if (useShrinkage)
						dividedIntervals.Add(new SkillStaffingIntervalLightModel
						{
							Id = skillStaffingInterval.SkillId,
							StartDateTime = skillStaffingInterval.StartDateTime,
							EndDateTime = skillStaffingInterval.EndDateTime,
							StaffingLevel = skillStaffingInterval.StaffingLevelWithShrinkage
						});
					else
						dividedIntervals.Add(new SkillStaffingIntervalLightModel
						{
							Id = skillStaffingInterval.SkillId,
							StartDateTime = skillStaffingInterval.StartDateTime,
							EndDateTime = skillStaffingInterval.EndDateTime,
							StaffingLevel = skillStaffingInterval.StaffingLevel
						});
				}
			}
			return dividedIntervals;
		}

		// From RequiredStaffingProvider
		private void mapWorkloadIds(ISkillDay clonedSkillDay, ISkillDay skillDay)
		{
			var index = 0;
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				clonedSkillDay.WorkloadDayCollection[index].Workload.SetId(workloadDay.Workload.Id.Value);
				index++;
			}
		}

		private void assignActualWorkloadToClonedSkillDay(ISkillDay clonedSkillDay, IList<SkillIntervalStatistics> skillDayStats, Dictionary<Guid, int> workloadBacklog)
		{
			clonedSkillDay.Lock();
			var skillDayStatsPerWorkload = skillDayStats.ToLookup(w => w.WorkloadId);
			foreach (var workloadDay in clonedSkillDay.WorkloadDayCollection)
			{
				if (workloadDay.OpenTaskPeriodList.Any())
				{
					var openHourStartLocal = workloadDay.OpenTaskPeriodList.First().Period.StartDateTime;
					var firstOpenInterval = skillDayStatsPerWorkload[workloadDay.Workload.Id.Value].FirstOrDefault(x => x.StartTime == openHourStartLocal);
					int calls = 0;
					if (firstOpenInterval != null && workloadBacklog.TryGetValue(workloadDay.Workload.Id.Value, out calls))
					{
						firstOpenInterval.Calls = calls + firstOpenInterval.Calls;
					}
				}
				else
					continue;
				foreach (var taskPeriod in workloadDay.TaskPeriodList)
				{
					var statInterval = skillDayStatsPerWorkload[workloadDay.Workload.Id.Value]
						.FirstOrDefault(x => x.StartTime == taskPeriod.Period.StartDateTime);

					taskPeriod.CampaignTasks = new Percent(0);
					taskPeriod.CampaignTaskTime = new Percent(0);
					taskPeriod.CampaignAfterTaskTime = new Percent(0);

					if (statInterval == null || double.IsNaN(statInterval.AverageHandleTime))
					{
						taskPeriod.SetTasks(0);
						taskPeriod.AverageTaskTime = TimeSpan.Zero;
						continue;
					}

					taskPeriod.SetTasks(statInterval.Calls);
					taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(statInterval.AverageHandleTime);
					taskPeriod.AverageAfterTaskTime = TimeSpan.Zero;
				}
			}
			clonedSkillDay.Release();
		}

		private void reset(ISkillDay clonedSkillDay, ISkillDay skillday)
		{
			clonedSkillDay.Lock();
			foreach (var workloadDay in clonedSkillDay.WorkloadDayCollection)
			{
				var innerWorkloadDay = skillday.WorkloadDayCollection.FirstOrDefault(x => x.Workload.Id.Value == workloadDay.Workload.Id.Value);
				foreach (var taskPeriod in workloadDay.TaskPeriodList)
				{
					var statInterval = innerWorkloadDay
						.TaskPeriodList.First(x => x.Period.StartDateTime == taskPeriod.Period.StartDateTime);

					taskPeriod.SetTasks(statInterval.Tasks);
					taskPeriod.AverageTaskTime = statInterval.Task.AverageTaskTime;
					taskPeriod.AverageAfterTaskTime = statInterval.Task.AverageAfterTaskTime;
				}
			}
			clonedSkillDay.Release();
		}

		private IList<StaffingInterval> GetRequiredStaffing(TimeSpan resolution, IList<SkillIntervalStatistics> skillDayStats,
			ISkillDay skillDay)
		{
			var actualStaffingIntervals = new List<StaffingInterval>();
			var staffing = skillDay.SkillStaffPeriodViewCollection(resolution, false);
			var skillIntervals = skillDayStats.GroupBy(x => x.StartTime);
			foreach (var interval in skillIntervals)
			{
				var utcTime = interval.Key;
				var staffingInterval = staffing.FirstOrDefault(x => x.Period.StartDateTime == utcTime);

				if (staffingInterval == null)
					continue;

				actualStaffingIntervals.Add(new StaffingInterval
				{
					SkillId = skillDay.Skill.Id.Value,
					StartTime = interval.Key,
					Agents = staffingInterval.FStaff
				});
			}

			return actualStaffingIntervals;
		}
	}
}
