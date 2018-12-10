using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{

	public class ScheduleOvertimeExecuteWrapper
	{
		private readonly ScheduleOvertimeWithoutStateHolder _scheduleOvertimeWithoutStateHolder;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly ICurrentScenario _currentScenario;
		public ScheduleOvertimeExecuteWrapper(ScheduleOvertimeWithoutStateHolder scheduleOvertimeWithoutStateHolder,
			ISkillCombinationResourceRepository skillCombinationResourceRepository, IIntervalLengthFetcher intervalLengthFetcher, ISkillDayLoadHelper skillDayLoadHelper, ICurrentScenario currentScenario)
		{
			_scheduleOvertimeWithoutStateHolder = scheduleOvertimeWithoutStateHolder;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_skillDayLoadHelper = skillDayLoadHelper;
			_currentScenario = currentScenario;
		}

		public OvertimeWrapperModel Execute(IOvertimePreferences overtimePreferences, ISchedulingProgress schedulingProgress,
			IScheduleDictionary scheduleDictionary, IEnumerable<IPerson> agents, DateOnlyPeriod period,
			DateTimePeriod requestedDateTimePeriod, IList<ISkill> skills, IList<ISkill> skillsToAddOvertime,
			DateTimePeriod fullDayPeriod)
		{
			var combinationResources = _skillCombinationResourceRepository
				.LoadSkillCombinationResources(new DateTimePeriod(fullDayPeriod.StartDateTime.AddDays(-2),
					fullDayPeriod.EndDateTime.AddDays(2))).ToList();

			// for email we need extra
			var skillDaysBySkills = _skillDayLoadHelper.LoadSchedulerSkillDays(period, skills, _currentScenario.Current());
			var skillStaffingIntervals = new HashSet<SkillStaffingInterval>();
			foreach (var skill in skillDaysBySkills.Keys)
			{
				var days = skillDaysBySkills[skill];
				foreach (var skillDay in days)
				{
					getSkillStaffingIntervals(skillDay).ForEach(i => skillStaffingIntervals.Add(i));
				}
			}
			
			skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var relevantSkillStaffPeriods =
				skillStaffingIntervals.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
					.ToDictionary(k => k.Key,
						v =>
							(IResourceCalculationPeriodDictionary)
							new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
								s => (IResourceCalculationPeriod) s)));
			var resCalcData = new ResourceCalculationData(skills,
				new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));

			using (getContext(combinationResources, skills, false))
			{
				var resourceCalculationPeriods = new List<SkillStaffingInterval>();
				var overtimeModels = new List<OverTimeModel>();
				var activites = skillsToAddOvertime.Select(x => x.Activity).Distinct();
				foreach (var activity in activites)
				{
					overtimePreferences.SkillActivity = activity;
					overtimeModels.AddRange(_scheduleOvertimeWithoutStateHolder.Execute(overtimePreferences, schedulingProgress,
						scheduleDictionary, agents, period,
						requestedDateTimePeriod, resCalcData, () => getContext(combinationResources, skills, true)));

				}

				var xx = resCalcData.SkillResourceCalculationPeriodDictionary.Items()
					.Where(x => skillsToAddOvertime.Contains(x.Key));
				var intervalLength = _intervalLengthFetcher.IntervalLength;
				foreach (var keyValuePair in xx)
				{
					var intervals = keyValuePair.Value.OnlyValues().Cast<SkillStaffingInterval>();
					var inPeriod = intervals.Where(x => fullDayPeriod.Contains(x.StartDateTime)).ToList();
					inPeriod = split(inPeriod, TimeSpan.FromMinutes(intervalLength)).ToList();
					resourceCalculationPeriods.AddRange(inPeriod);
				}

				var grouped = resourceCalculationPeriods.GroupBy(l => l.StartDateTime)
					.Select(cl => new SkillStaffingInterval
					{
						StartDateTime = cl.First().StartDateTime,
						EndDateTime =  cl.First().EndDateTime,
						FStaff = cl.Sum(c => c.FStaff),
						StaffingLevel = cl.Sum(c => c.StaffingLevel),
						SkillId = skillsToAddOvertime.First().Id.GetValueOrDefault()
					}).OrderBy(x => x.StartDateTime).ToList<SkillStaffingInterval>();

				return new OvertimeWrapperModel(grouped, overtimeModels);
			}
		}


		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, IList<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}

		private IEnumerable<SkillStaffingInterval> getSkillStaffingIntervals(ISkillDay skillDay)
		{
			var skillStaffPeriods = skillDay.SkillStaffPeriodCollection;

			return skillStaffPeriods.Select(skillStaffPeriod => new SkillStaffingInterval
			{
				SkillId = skillDay.Skill.Id.GetValueOrDefault(),
				StartDateTime = skillStaffPeriod.Period.StartDateTime,
				EndDateTime = skillStaffPeriod.Period.EndDateTime,
				Forecast = skillStaffPeriod.FStaff,
				StaffingLevel = 0,
			});
		}

		private static IList<SkillStaffingInterval> split(List<SkillStaffingInterval> skillStaffingIntervals, TimeSpan resolution)
		{
			var dividedIntervals = new List<SkillStaffingInterval>();

			foreach (var interval in skillStaffingIntervals)
			{
				if (!interval.GetTimeSpan().Equals(resolution))
				{
					dividedIntervals.AddRange(splitInterval(interval, resolution));
				}
				else
				{
					dividedIntervals.Add(interval);
				}
			}
			return dividedIntervals;
		}

		private static IEnumerable<SkillStaffingInterval> splitInterval(SkillStaffingInterval interval, TimeSpan resolution)
		{
			var dividedIntervals = new List<SkillStaffingInterval>();
			var startInterval = interval.StartDateTime;
			while (startInterval < interval.EndDateTime)
			{

				dividedIntervals.Add(new SkillStaffingInterval
				{
					StartDateTime = startInterval,
					EndDateTime = startInterval.Add(resolution),
					SkillId = interval.SkillId,
					FStaff = interval.FStaff,
					StaffingLevel = interval.StaffingLevel
				});
				startInterval = startInterval.Add(resolution);
			}
			return dividedIntervals;
		}
	}

	public class OvertimeWrapperModel
	{
		public List<SkillStaffingInterval> ResourceCalculationPeriods { get; }
		public IList<OverTimeModel> Models { get; }

		public OvertimeWrapperModel(List<SkillStaffingInterval> resourceCalculationPeriods, IList<OverTimeModel> models)
		{
			ResourceCalculationPeriods = resourceCalculationPeriods;
			Models = models;
		}
	}
	
}
