using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class SkillStaffingIntervalProvider
	{
		private readonly SplitSkillStaffInterval _splitSkillStaffInterval;
		private readonly ISkillRepository _skillRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ExtractSkillForecastIntervals _extractSkillForecastIntervals;

		public SkillStaffingIntervalProvider(SplitSkillStaffInterval splitSkillStaffInterval,
											 ISkillCombinationResourceRepository skillCombinationResourceRepository, ISkillRepository skillRepository, IResourceCalculation resourceCalculation, ExtractSkillForecastIntervals extractSkillForecastIntervals, IActivityRepository activityRepository,
											 ISkillDayRepository skillDayRepository,
											 ICurrentScenario currentScenario)
		{
			_splitSkillStaffInterval = splitSkillStaffInterval;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillRepository = skillRepository;
			_resourceCalculation = resourceCalculation;
			_extractSkillForecastIntervals = extractSkillForecastIntervals;
			_activityRepository = activityRepository;
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
		}

		public IList<SkillStaffingIntervalLightModel> StaffingForSkills(Guid[] skillIdList, DateTimePeriod period, TimeSpan resolution, bool useShrinkage)
		{
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			if (!combinationResources.Any())
			{
				return new List<SkillStaffingIntervalLightModel>();
			}

			var allSkillStaffingIntervals = GetSkillStaffIntervalsAllSkills(period, combinationResources, useShrinkage);
			var relevantSkillStaffingIntervals = allSkillStaffingIntervals.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime && skillIdList.Contains(x.SkillId)).ToList();
			var splittedIntervals = _splitSkillStaffInterval.Split(relevantSkillStaffingIntervals, resolution, false); 
			return splittedIntervals;

		}

		public IList<SkillStaffingInterval> StaffingIntervalsForSkills(Guid[] skillIdList, DateTimePeriod period,
			TimeSpan resolution, bool useShrinkage)
		{
			// if combinationResources is empty, Scheduled agents of intervals will be assumed as 0.00
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			var allSkillStaffingIntervals = GetSkillStaffIntervalsAllSkills(period, combinationResources, useShrinkage);
			var relevantSkillStaffingIntervals = allSkillStaffingIntervals.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime && skillIdList.Contains(x.SkillId)).ToList();
			return relevantSkillStaffingIntervals;
		}

		public IList<SkillStaffingInterval> StaffingIntervalsForSkills(Guid?[] skillIdList, DateTimePeriod period, bool useShrinkage)
		{
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			if (!combinationResources.Any())
			{
				return new List<SkillStaffingInterval>();
			}

			var allSkillStaffingIntervals = GetSkillStaffIntervalsAllSkills(period, combinationResources, useShrinkage);
			var relevantSkillStaffingIntervals = allSkillStaffingIntervals.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime && skillIdList.Contains(x.SkillId)).ToList();
			return relevantSkillStaffingIntervals;
		}

		public List<SkillStaffingInterval> GetSkillStaffIntervalsAllSkills(DateTimePeriod period, List<SkillCombinationResource> combinationResources, bool useShrinkage)
		{
			_activityRepository.LoadAll();
			var skills = _skillRepository.LoadAll().ToList();

			var skillStaffingIntervals = _extractSkillForecastIntervals.GetBySkills(skills, period, useShrinkage).ToList();
			skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var relevantSkillStaffPeriods =
				skillStaffingIntervals.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
					.ToDictionary(k => k.Key,
								  v =>
									  (IResourceCalculationPeriodDictionary)
									  new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
																							 s => (IResourceCalculationPeriod)s)));
			var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));
			//var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfo.Utc);
		    var dateOnlyPeriod = ExtractSkillForecastIntervals.GetLongestPeriod(skills, period);

            using (getContext(combinationResources, skills, false))
			{
				_resourceCalculation.ResourceCalculate(dateOnlyPeriod, resCalcData, () => getContext(combinationResources, skills, true));
			}

			SetSkillStaffPeriodWithFStaffAndEsl(skillStaffingIntervals, skills, period, TimeZoneInfo.Utc, useShrinkage);
			return skillStaffingIntervals;
		}

		private void SetSkillStaffPeriodWithFStaffAndEsl(List<SkillStaffingInterval> skillStaffingIntervals, List<ISkill> skills, DateTimePeriod period, TimeZoneInfo timeZone, bool useShrinkage)
		{
			var skillDays = _skillDayRepository.FindReadOnlyRange(period.ToDateOnlyPeriod(timeZone), skills, _currentScenario.Current());

			foreach (var skill in skills)
			{
				var staffingIntervals = skillStaffingIntervals.Where(x => x.SkillId == skill.Id.Value);
				foreach (var skillDay in skillDays.Where(x => x.Skill.Id == skill.Id))
				{
					foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
					{
						var intervalStartLocal = skillStaffPeriod.Period.StartDateTime;
						var scheduledStaff =
							staffingIntervals.FirstOrDefault(x => x.StartDateTime == intervalStartLocal);
						if (scheduledStaff == null)
							continue;
						skillStaffPeriod.SetCalculatedResource65(0);
						if (scheduledStaff.StaffingLevel > 0)
						{
							skillStaffPeriod.SetCalculatedResource65(useShrinkage
								? scheduledStaff.StaffingLevelWithShrinkage
								: scheduledStaff.StaffingLevel);
						}
					}
				}

				foreach (var skillStaffingInterval in staffingIntervals)
				{
					var skillDay = skillDays.FirstOrDefault(x => x.Skill.Id == skill.Id && x.CurrentDate.Date ==
																 skillStaffingInterval.StartDateTime.Date);
					var skillStaffPeriod = skillDay?.SkillStaffPeriodCollection
						.FirstOrDefault(y => y.Period.StartDateTime == skillStaffingInterval.StartDateTime);
					if (skillStaffPeriod == null)
						continue;
					skillStaffingInterval.Forecast = skillStaffPeriod.FStaff;
					skillStaffingInterval.EstimatedServiceLevel = skillStaffPeriod.EstimatedServiceLevel;
				}
			}
		}


		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, List<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}

	
	}
}
