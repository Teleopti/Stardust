﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceOptimizationHelper : IResourceOptimizationHelper
	{
		private readonly IOccupiedSeatCalculator _occupiedSeatCalculator;
		private readonly INonBlendSkillCalculator _nonBlendSkillCalculator;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IPeriodDistributionService _periodDistributionService;
		private readonly IIntraIntervalFinderService _intraIntervalFinderService;
		private readonly ITimeZoneGuard _timeZoneGuard;
		private readonly IResourceCalculationContextFactory _resourceCalculationContextFactory;

		public ResourceOptimizationHelper(IOccupiedSeatCalculator occupiedSeatCalculator,
			INonBlendSkillCalculator nonBlendSkillCalculator,
			Func<IPersonSkillProvider> personSkillProvider,
			IPeriodDistributionService periodDistributionService,
			IIntraIntervalFinderService intraIntervalFinderService,
			ITimeZoneGuard timeZoneGuard,
			IResourceCalculationContextFactory resourceCalculationContextFactory)
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
					context = _resourceCalculationContextFactory.Create(resourceCalculationData.Schedules, resourceCalculationData.Skills, new DateOnlyPeriod(localDate.AddDays(-1), localDate.AddDays(1)));
				}
				var relevantProjections = ResourceCalculationContext.Fetch();
				resourceCalculateDate(resourceCalculationData, relevantProjections, localDate, resourceCalculationData.ConsiderShortBreaks);

				if (resourceCalculationData.DoIntraIntervalCalculation)
				{
					_intraIntervalFinderService.Execute(resourceCalculationData.SkillDays, localDate, relevantProjections);
				}

				if (context != null)
				{
					context.Dispose();
				}
			}
		}

		private DateTimePeriod getPeriod(DateOnly localDate)
		{
			var currentStart = localDate;
			return new DateOnlyPeriod(currentStart, currentStart).ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()); 
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
					resourceCalculationData.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
					ordinarySkills, timePeriod);

			var schedulingResultService = new SchedulingResultService(relevantSkillStaffPeriods,
				resourceCalculationData.Skills,
				relevantProjections, _personSkillProvider());

			schedulingResultService.SchedulingResult(timePeriod);

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
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods =
				createSkillSkillStaffDictionaryOnSkills(
					resourceCalculationData.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
					maxSeatSkills, timePeriod);
			_occupiedSeatCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods);

			var nonBlendSkills =
				resourceCalculationData.Skills.Where(skill => skill.SkillType.ForecastSource == ForecastSource.NonBlendSkill)
					.ToList();
			relevantSkillStaffPeriods =
				createSkillSkillStaffDictionaryOnSkills(
					resourceCalculationData.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, nonBlendSkills, timePeriod);
			_nonBlendSkillCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods, false);
		}

		private static ISkillSkillStaffPeriodExtendedDictionary createSkillSkillStaffDictionaryOnSkills(
			ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary, IList<ISkill> skills, DateTimePeriod keyPeriod)
		{
			var result = new SkillSkillStaffPeriodExtendedDictionary();
			foreach (var skill in skills)
			{
				ISkillStaffPeriodDictionary skillStaffDictionary;
				if (!skillStaffPeriodDictionary.TryGetValue(skill, out skillStaffDictionary)) continue;

				var skillStaffPeriodDictionaryToReturn = new SkillStaffPeriodDictionary(skill);
				foreach (var skillStaffPeriod in skillStaffDictionary)
				{
					if (!skillStaffPeriod.Key.Intersect(keyPeriod)) continue;
					skillStaffPeriodDictionaryToReturn.Add(skillStaffPeriod.Key, skillStaffPeriod.Value);
				}
				result.Add(skill, skillStaffPeriodDictionaryToReturn);
			}
			return result;
		}
	}
}
