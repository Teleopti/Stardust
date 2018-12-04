using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class SkillStaffingIntervalProvider : ISkillStaffingIntervalProvider
	{
		private readonly SplitSkillStaffInterval _splitSkillStaffInterval;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ExtractSkillForecastIntervals _extractSkillForecastIntervals;

		public SkillStaffingIntervalProvider(SplitSkillStaffInterval splitSkillStaffInterval,
											 ISkillCombinationResourceRepository skillCombinationResourceRepository, ISkillRepository skillRepository, IResourceCalculation resourceCalculation, ExtractSkillForecastIntervals extractSkillForecastIntervals)
		{
			_splitSkillStaffInterval = splitSkillStaffInterval;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillRepository = skillRepository;
			_resourceCalculation = resourceCalculation;
			_extractSkillForecastIntervals = extractSkillForecastIntervals;
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

		public IList<SkillStaffingInterval> StaffingIntervalsForSkills(Guid[] skillIdList, DateTimePeriod period, bool useShrinkage, bool useBpoStaffing=true)
		{
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period, useBpoStaffing).ToList();

			var allSkillStaffingIntervals = GetSkillStaffIntervalsAllSkills(period, combinationResources, useShrinkage);
			var relevantSkillStaffingIntervals = allSkillStaffingIntervals.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime && skillIdList.Contains(x.SkillId)).ToList();
			return relevantSkillStaffingIntervals;
		}

		public List<SkillStaffingInterval> GetSkillStaffIntervalsAllSkills(DateTimePeriod periodUtc, List<SkillCombinationResource> combinationResources, bool useShrinkage)
		{
			var skills = _skillRepository.LoadAll().ToList();
			var skillStaffingIntervals = _extractSkillForecastIntervals.GetBySkills(skills, periodUtc, useShrinkage).ToList();
			skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);
			
			var relevantSkillStaffPeriods =
				skillStaffingIntervals.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
					.ToDictionary(k => k.Key,
								  v =>
									  (IResourceCalculationPeriodDictionary)
									  new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
																							 s => (IResourceCalculationPeriod)s)));
			var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));

			using (getContext(combinationResources, skills, false))
			{
				var dateOnlyPeriod = ExtractSkillForecastIntervals.GetLongestPeriod(skills, periodUtc);
				_resourceCalculation.ResourceCalculate(dateOnlyPeriod, resCalcData, () => getContext(combinationResources, skills, true));
			}
			return skillStaffingIntervals;
		}

		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, List<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}
	}
}
