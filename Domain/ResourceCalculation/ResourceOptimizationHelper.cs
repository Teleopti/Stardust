using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	// This class is registered in servicebus and sdk as well
	public class ResourceOptimizationHelper : IResourceOptimizationHelper
	{
		private readonly Func<ISchedulingResultStateHolder> _stateHolder;
		private readonly IOccupiedSeatCalculator _occupiedSeatCalculator;
		private readonly INonBlendSkillCalculator _nonBlendSkillCalculator;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IPeriodDistributionService _periodDistributionService;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IIntraIntervalFinderService _intraIntervalFinderService;


		public ResourceOptimizationHelper(Func<ISchedulingResultStateHolder> stateHolder,
			IOccupiedSeatCalculator occupiedSeatCalculator,
			INonBlendSkillCalculator nonBlendSkillCalculator,
			Func<IPersonSkillProvider> personSkillProvider,
			IPeriodDistributionService periodDistributionService,
			ICurrentTeleoptiPrincipal currentTeleoptiPrincipal,
			IIntraIntervalFinderService intraIntervalFinderService)
		{
			_stateHolder = stateHolder;
			_occupiedSeatCalculator = occupiedSeatCalculator;
			_nonBlendSkillCalculator = nonBlendSkillCalculator;
			_personSkillProvider = personSkillProvider;
			_periodDistributionService = periodDistributionService;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_intraIntervalFinderService = intraIntervalFinderService;
		}

		public void ResourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks)
		{
			resourceCalculateDate(localDate, useOccupancyAdjustment, considerShortBreaks);
		}

		private void resourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks)
		{
			if (_stateHolder().TeamLeaderMode)
				return;

			if (_stateHolder().SkipResourceCalculation)
				return;

			if (_stateHolder().Skills.Count.Equals(0))
				return;

			using (PerformanceOutput.ForOperation("ResourceCalculate " + localDate.ToShortDateString()))
			{
			    IResourceCalculationDataContainerWithSingleOperation relevantProjections;
			    IDisposable context = null;
			    if (ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>.InContext)
			    {
			        relevantProjections = ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>.Container();
			    }
			    else
			    {
			        var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), _stateHolder().Skills.Min(s => s.DefaultResolution));
			        relevantProjections = extractor.CreateRelevantProjectionList(_stateHolder().Schedules,
			                                                                     TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
			                                                                         localDate.AddDays(-1).Date, localDate.AddDays(1).Date));
			        context = new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(relevantProjections);
			    }

				
				ResourceCalculateDate(relevantProjections, localDate, useOccupancyAdjustment, considerShortBreaks);

				_intraIntervalFinderService.Execute(_stateHolder(), localDate, relevantProjections);
				

                if (context != null)
                {
                    context.Dispose();
                }
			}
		}

		private DateTimePeriod getPeriod(DateOnly localDate)
		{
			var currentStart = localDate;
			return new DateOnlyPeriod(currentStart, currentStart).ToDateTimePeriod(_currentTeleoptiPrincipal.Current().Regional.TimeZone);
		}

		public void ResourceCalculateDate(IResourceCalculationDataContainer relevantProjections,
		                                   DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks)
		{
			var timePeriod = getPeriod(localDate);
			var ordinarySkills = new List<ISkill>();
			foreach (var skill in _stateHolder().Skills)
			{
				if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill &&
				    skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
					ordinarySkills.Add(skill);
			}

			var relevantSkillStaffPeriods =
				CreateSkillSkillStaffDictionaryOnSkills(_stateHolder().SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
				                                        ordinarySkills, timePeriod);

			var schedulingResultService = new SchedulingResultService(relevantSkillStaffPeriods, _stateHolder().Skills,
			                                                          relevantProjections, useOccupancyAdjustment, _personSkillProvider());

			schedulingResultService.SchedulingResult(timePeriod);

			if (considerShortBreaks)
			{
				_periodDistributionService.CalculateDay(relevantProjections, relevantSkillStaffPeriods);
			}

			calculateMaxSeatsAndNonBlend(relevantProjections, localDate, timePeriod);
		}

		private void calculateMaxSeatsAndNonBlend(IResourceCalculationDataContainer relevantProjections, DateOnly localDate, DateTimePeriod timePeriod)
		{
			var maxSeatSkills = _stateHolder().Skills.Where(skill => skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill).ToList();
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = CreateSkillSkillStaffDictionaryOnSkills(_stateHolder().SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
			                                                                                                             maxSeatSkills, timePeriod);
			_occupiedSeatCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods);
			
			var nonBlendSkills = _stateHolder().Skills.Where(skill => skill.SkillType.ForecastSource == ForecastSource.NonBlendSkill).ToList();
			relevantSkillStaffPeriods = CreateSkillSkillStaffDictionaryOnSkills(_stateHolder().SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, nonBlendSkills, timePeriod);
			_nonBlendSkillCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods, false);
		}

		public ISkillSkillStaffPeriodExtendedDictionary CreateSkillSkillStaffDictionaryOnSkills(ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary, IList<ISkill> skills, DateTimePeriod keyPeriod)
		{
			var result = new SkillSkillStaffPeriodExtendedDictionary();
			foreach (var skillPair in skillStaffPeriodDictionary)
			{
				if (!skills.Contains(skillPair.Key))
					continue;
				ISkillStaffPeriodDictionary skillStaffDictionary = skillPair.Value;

				var skillStaffPeriodDictionaryToReturn = new SkillStaffPeriodDictionary(skillPair.Key);
				foreach (var skillStaffPeriod in skillStaffDictionary)
				{
					if (!skillStaffPeriod.Key.Intersect(keyPeriod)) continue;
					skillStaffPeriodDictionaryToReturn.Add(skillStaffPeriod.Key, skillStaffPeriod.Value);
				}
				result.Add(skillPair.Key, skillStaffPeriodDictionaryToReturn);
			}

			return result;
		}
	}
}
