﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

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
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;

		public ResourceOptimizationHelper(IOccupiedSeatCalculator occupiedSeatCalculator,
			INonBlendSkillCalculator nonBlendSkillCalculator,
			IPersonSkillProvider personSkillProvider,
			IPeriodDistributionService periodDistributionService,
			IIntraIntervalFinderService intraIntervalFinderService,
			ITimeZoneGuard timeZoneGuard,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_occupiedSeatCalculator = occupiedSeatCalculator;
			_nonBlendSkillCalculator = nonBlendSkillCalculator;
			_personSkillProvider = personSkillProvider;
			_periodDistributionService = periodDistributionService;
			_intraIntervalFinderService = intraIntervalFinderService;
			_timeZoneGuard = timeZoneGuard;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}

		public void ResourceCalculate(DateOnly localDate, IResourceCalculationData resourceCalculationData)
		{
			if (resourceCalculationData.SkipResourceCalculation)
				return;

			if (!resourceCalculationData.Skills.Any())
				return;

			using (PerformanceOutput.ForOperation("ResourceCalculate " + localDate.ToShortDateString()))
			{
				IDisposable context = null;
				if (!ResourceCalculationContext.InContext)
				{
					context = _resourceCalculationContextFactory.Create(resourceCalculationData.Schedules, resourceCalculationData.Skills, false, localDate.ToDateOnlyPeriod());
				}
				var relevantProjections = ResourceCalculationContext.Fetch();
				resourceCalculateDate(resourceCalculationData, relevantProjections, localDate, resourceCalculationData.ConsiderShortBreaks);

				if (resourceCalculationData.DoIntraIntervalCalculation)
				{
					_intraIntervalFinderService.Execute(resourceCalculationData.SkillDays, localDate, relevantProjections);
				}

				context?.Dispose();
			}
		}

		private DateTimePeriod getPeriod(DateOnly localDate)
		{
			return localDate.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()); 
		}

		private void resourceCalculateDate(IResourceCalculationData resourceCalculationData, IResourceCalculationDataContainer relevantProjections, DateOnly localDate, bool considerShortBreaks)
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

		private void calculateMaxSeatsAndNonBlend(IResourceCalculationData resourceCalculationData, IResourceCalculationDataContainer relevantProjections, DateOnly localDate,
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
				IResourceCalculationPeriodDictionary skillStaffDictionary;
				if (!skillStaffPeriodDictionary.TryGetValue(skill, out skillStaffDictionary)) continue;

				var skillStaffPeriodDictionaryToReturn = new Dictionary<DateTimePeriod, IResourceCalculationPeriod>();
				foreach (var skillStaffPeriod in skillStaffDictionary.Items())
				{
					if (!skillStaffPeriod.Key.Intersect(keyPeriod)) continue;
					skillStaffPeriodDictionaryToReturn.Add(skillStaffPeriod.Key, skillStaffPeriod.Value);
				}

				result.Add(new KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>(skill, new ResourceCalculationPeriodDictionary(skillStaffPeriodDictionaryToReturn)));
			}
			return new SkillResourceCalculationPeriodWrapper(result);
		}
	}
}
