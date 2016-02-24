using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceOptimizationHelper : IResourceOptimizationHelper
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly IOccupiedSeatCalculator _occupiedSeatCalculator;
		private readonly INonBlendSkillCalculator _nonBlendSkillCalculator;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IPeriodDistributionService _periodDistributionService;
		private readonly IIntraIntervalFinderService _intraIntervalFinderService;

		public ResourceOptimizationHelper(Func<ISchedulerStateHolder> stateHolder,
			IOccupiedSeatCalculator occupiedSeatCalculator,
			INonBlendSkillCalculator nonBlendSkillCalculator,
			Func<IPersonSkillProvider> personSkillProvider,
			IPeriodDistributionService periodDistributionService,
			IIntraIntervalFinderService intraIntervalFinderService)
		{
			_stateHolder = stateHolder;
			_occupiedSeatCalculator = occupiedSeatCalculator;
			_nonBlendSkillCalculator = nonBlendSkillCalculator;
			_personSkillProvider = personSkillProvider;
			_periodDistributionService = periodDistributionService;
			_intraIntervalFinderService = intraIntervalFinderService;
		}


		public void ResourceCalculateDate(DateOnly localDate, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			var stateHolder = _stateHolder();
			if (stateHolder.SchedulingResultState.TeamLeaderMode)
				return;

			if (stateHolder.SchedulingResultState.SkipResourceCalculation)
				return;

			if (stateHolder.SchedulingResultState.Skills.Length == 0)
				return;

			using (PerformanceOutput.ForOperation("ResourceCalculate " + localDate.ToShortDateString()))
			{
				IResourceCalculationDataContainerWithSingleOperation relevantProjections;
				IDisposable context = null;
				if (ResourceCalculationContext.InContext)
				{
					relevantProjections = ResourceCalculationContext.Fetch();
				}
				else
				{
					var extractor = new ScheduleProjectionExtractor(_personSkillProvider(),
						stateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution));
					relevantProjections = extractor.CreateRelevantProjectionList(stateHolder.Schedules,
						TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
							localDate.AddDays(-1).Date, localDate.AddDays(1).Date, stateHolder.TimeZoneInfo));
					context = new ResourceCalculationContext(relevantProjections);
				}

				ResourceCalculateDate(relevantProjections, localDate, considerShortBreaks);

				if (doIntraIntervalCalculation)
				{
					_intraIntervalFinderService.Execute(stateHolder.SchedulingResultState, localDate, relevantProjections);
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
			return new DateOnlyPeriod(currentStart, currentStart).ToDateTimePeriod(TimeZoneGuard.Instance.TimeZone);
		}

		public void ResourceCalculateDate(IResourceCalculationDataContainer relevantProjections,
			DateOnly localDate, bool considerShortBreaks)
		{

			var timePeriod = getPeriod(localDate);
			var ordinarySkills = new List<ISkill>();
			var schedulerStateHolder = _stateHolder();
			foreach (var skill in schedulerStateHolder.SchedulingResultState.Skills)
			{
				if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill &&
					skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
					ordinarySkills.Add(skill);
			}

			var relevantSkillStaffPeriods =
				CreateSkillSkillStaffDictionaryOnSkills(
					schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
					ordinarySkills, timePeriod);

			var schedulingResultService = new SchedulingResultService(relevantSkillStaffPeriods,
				schedulerStateHolder.SchedulingResultState.Skills,
				relevantProjections, _personSkillProvider());

			schedulingResultService.SchedulingResult(timePeriod);

			if (considerShortBreaks)
			{
				_periodDistributionService.CalculateDay(relevantProjections, relevantSkillStaffPeriods);
			}

			calculateMaxSeatsAndNonBlend(relevantProjections, localDate, timePeriod);
		}

		private void calculateMaxSeatsAndNonBlend(IResourceCalculationDataContainer relevantProjections, DateOnly localDate,
			DateTimePeriod timePeriod)
		{
			var schedulingResultStateHolder = _stateHolder().SchedulingResultState;
			var maxSeatSkills =
				schedulingResultStateHolder.Skills.Where(skill => skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
					.ToList();
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods =
				CreateSkillSkillStaffDictionaryOnSkills(
					schedulingResultStateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
					maxSeatSkills, timePeriod);
			_occupiedSeatCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods);

			var nonBlendSkills =
				schedulingResultStateHolder.Skills.Where(skill => skill.SkillType.ForecastSource == ForecastSource.NonBlendSkill)
					.ToList();
			relevantSkillStaffPeriods =
				CreateSkillSkillStaffDictionaryOnSkills(
					schedulingResultStateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, nonBlendSkills, timePeriod);
			_nonBlendSkillCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods, false);
		}

		public ISkillSkillStaffPeriodExtendedDictionary CreateSkillSkillStaffDictionaryOnSkills(
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
