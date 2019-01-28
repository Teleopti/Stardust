using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Intraday.To_Staffing
{
	public class ResourceCalculationUsingReadModels
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillForecastReadModelRepository _skillForecastReadModelRepository;
		private readonly IResourceCalculation _resourceCalculation;

		public ResourceCalculationUsingReadModels(ISkillRepository skillRepository, ISkillCombinationResourceRepository skillCombinationResourceRepository, 
			ISkillForecastReadModelRepository skillForecastReadModelRepository, IResourceCalculation resourceCalculation)
		{
			_skillRepository = skillRepository;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillForecastReadModelRepository = skillForecastReadModelRepository;
			_resourceCalculation = resourceCalculation;
		}
		
		public IList<SkillStaffingInterval> LoadAndResourceCalculate(IEnumerable<Guid> skillIds, DateTime startOfDayUtc, DateTime endOfDayUtc, bool useShrinkage)
		{
			var skills = _skillRepository.LoadAll().ToList();
			var firstPeriodDateInSkillTimeZone = new DateOnly(skills.Select(x => TimeZoneInfo.ConvertTimeFromUtc(startOfDayUtc, x.TimeZone)).Min());
			var lastPeriodDateInSkillTimeZone = new DateOnly(skills.Select(x => TimeZoneInfo.ConvertTimeFromUtc(startOfDayUtc.AddDays(1), x.TimeZone)).Max());
			var dateOnlyPeriod = new DateOnlyPeriod(firstPeriodDateInSkillTimeZone, lastPeriodDateInSkillTimeZone);

			var period = new DateTimePeriod(startOfDayUtc, endOfDayUtc);
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			
			var skillForecastList =
				_skillForecastReadModelRepository.LoadSkillForecast(skillIds.ToArray(), period);

			var intervals = skillForecastList.Select(skillForecast => new SkillStaffingInterval
			{
				SkillId = skillForecast.SkillId,
				StartDateTime = skillForecast.StartDateTime,
				EndDateTime = skillForecast.EndDateTime,
				Forecast = useShrinkage ? skillForecast.AgentsWithShrinkage : skillForecast.Agents,
				StaffingLevel = 0,
				IsBacklogType = skillForecast.IsBackOffice
			}).ToList();
			
			calculateForecastAgentsForEmailSkills(intervals,combinationResources);
			var returnList = new HashSet<SkillStaffingInterval>();
			intervals.ForEach(i => returnList.Add(i));

			var skillStaffingIntervals = returnList
				.Where(x => period.Contains(x.StartDateTime) || x.DateTimePeriod.Contains(period.StartDateTime)).ToList();
			skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var relevantSkillStaffPeriods = skillStaffingIntervals
				.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
				.ToDictionary(k => k.Key,
					v =>
						(IResourceCalculationPeriodDictionary)
						new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
							s => (IResourceCalculationPeriod)s)));

			var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));

			using (new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(
				() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, false))))
			{
				_resourceCalculation
					.ResourceCalculate(dateOnlyPeriod, resCalcData, 
						() => new ResourceCalculationContext(
							new Lazy<IResourceCalculationDataContainerWithSingleOperation>(
								() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, true))));
			}

			return skillStaffingIntervals;
		}
		
		private static void calculateForecastAgentsForEmailSkills(IList<SkillStaffingInterval> skillStaffingIntervals, IList<SkillCombinationResource> skillCombinationResources)
		{
			var backlogSkillStaffingIntervals = skillStaffingIntervals.Where(s => s.IsBacklogType);

			foreach (var interval in backlogSkillStaffingIntervals)
			{
				var skillId = interval.SkillId;
				var skillCombinationsForSkill = skillCombinationResources.Where(s => s.SkillCombination.Contains(skillId));
				interval.SetCalculatedResource65(0);
				var totalResources = skillCombinationsForSkill.Where(s => s.StartDateTime == interval.StartDateTime)
					.Sum(s => s.Resource);
				
				if(totalResources > 0)
					interval.SetCalculatedResource65(totalResources);
			}
		}
	}
}