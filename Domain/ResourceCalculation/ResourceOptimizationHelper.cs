using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceOptimizationHelper
	{
		private readonly IOccupiedSeatCalculator _occupiedSeatCalculator;
		private readonly INonBlendSkillCalculator _nonBlendSkillCalculator;
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IPeriodDistributionService _periodDistributionService;
		private readonly IIntraIntervalFinderService _intraIntervalFinderService;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ResourceOptimizationHelper(IOccupiedSeatCalculator occupiedSeatCalculator,
			INonBlendSkillCalculator nonBlendSkillCalculator,
			IPersonSkillProvider personSkillProvider,
			IPeriodDistributionService periodDistributionService,
			IIntraIntervalFinderService intraIntervalFinderService,
			ITimeZoneGuard timeZoneGuard)
		{
			_occupiedSeatCalculator = occupiedSeatCalculator;
			_nonBlendSkillCalculator = nonBlendSkillCalculator;
			_personSkillProvider = personSkillProvider;
			_periodDistributionService = periodDistributionService;
			_intraIntervalFinderService = intraIntervalFinderService;
			_timeZoneGuard = timeZoneGuard;
		}

		public virtual void ResourceCalculate(DateOnly localDate, ResourceCalculationData resourceCalculationData)
		{
			var relevantProjections = ResourceCalculationContext.Fetch();
			resourceCalculateDate(resourceCalculationData, relevantProjections, localDate, resourceCalculationData.ConsiderShortBreaks);

			if (resourceCalculationData.DoIntraIntervalCalculation)
			{
				_intraIntervalFinderService.Execute(resourceCalculationData.SkillDays, localDate, relevantProjections);
			}
		}

		private DateTimePeriod getPeriod(DateOnly localDate)
		{
			return localDate.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()); 
		}

		private void resourceCalculateDate(ResourceCalculationData resourceCalculationData, IResourceCalculationDataContainer relevantProjections, DateOnly localDate, bool considerShortBreaks)
		{
			var timePeriod = getPeriod(localDate);
			var ordinarySkills = new List<ISkill>();
			foreach (var skill in resourceCalculationData.Skills)
			{
				if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill &&
					skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
					ordinarySkills.Add(skill);
			}

			var relevantSkillStaffPeriods =
				createSkillSkillStaffDictionaryOnSkills(
					resourceCalculationData.SkillResourceCalculationPeriodDictionary,
					ordinarySkills, timePeriod);

			var schedulingResultService = new SchedulingResultService(relevantSkillStaffPeriods,
				resourceCalculationData.Skills,
				relevantProjections, _personSkillProvider);

			schedulingResultService.SchedulingResult(timePeriod, resourceCalculationData, false);

			if (considerShortBreaks)
			{
				_periodDistributionService.CalculateDay(relevantProjections, relevantSkillStaffPeriods);
			}

			calculateMaxSeatsAndNonBlend(resourceCalculationData, relevantProjections, localDate, timePeriod);
		}

		private void calculateMaxSeatsAndNonBlend(ResourceCalculationData resourceCalculationData, IResourceCalculationDataContainer relevantProjections, DateOnly localDate,
			DateTimePeriod timePeriod)
		{
			var maxSeatSkills =
				resourceCalculationData.Skills.Where(skill => skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
					.ToList();
			ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods =
				createSkillSkillStaffDictionaryOnSkills(
					resourceCalculationData.SkillResourceCalculationPeriodDictionary,
					maxSeatSkills, timePeriod);
			_occupiedSeatCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods);

			var nonBlendSkills =
				resourceCalculationData.Skills.Where(skill => skill.SkillType.ForecastSource == ForecastSource.NonBlendSkill)
					.ToList();
			relevantSkillStaffPeriods =
				createSkillSkillStaffDictionaryOnSkills(
					resourceCalculationData.SkillResourceCalculationPeriodDictionary, nonBlendSkills, timePeriod);
			_nonBlendSkillCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods, false);
		}

		private static ISkillResourceCalculationPeriodDictionary createSkillSkillStaffDictionaryOnSkills(
			ISkillResourceCalculationPeriodDictionary skillStaffPeriodDictionary, IList<ISkill> skills, DateTimePeriod keyPeriod)
		{
			var result = new List<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>>();

			foreach (var skill in skills)
			{
				if (!skillStaffPeriodDictionary.TryGetValue(skill, out var skillStaffDictionary)) continue;

				var skillStaffPeriodDictionaryToReturn = skillStaffDictionary.Items()
					.Where(skillStaffPeriod => skillStaffPeriod.Key.Intersect(keyPeriod))
					.ToDictionary(k => k.Key, v => v.Value);

				result.Add(new KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>(skill, new ResourceCalculationPeriodDictionary(skillStaffPeriodDictionaryToReturn)));
			}
			return new SkillResourceCalculationPeriodWrapper(result);
		}
	}
}
