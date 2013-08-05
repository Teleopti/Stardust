
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{

	public class ResourceOptimizationHelper : IResourceOptimizationHelper
	{
		private readonly ISchedulingResultStateHolder _stateHolder;
		private readonly IOccupiedSeatCalculator _occupiedSeatCalculator;
		private readonly INonBlendSkillCalculator _nonBlendSkillCalculator;
		private readonly ISingleSkillDictionary _singleSkillDictionary;
	    private readonly ISingleSkillMaxSeatCalculator _singleSkillMaxSeatCalculator;
		private readonly IPersonSkillProvider _personSkillProvider;

		public ResourceOptimizationHelper(ISchedulingResultStateHolder stateHolder,
			IOccupiedSeatCalculator occupiedSeatCalculator,
			INonBlendSkillCalculator nonBlendSkillCalculator,
			ISingleSkillDictionary singleSkillDictionary,
			ISingleSkillMaxSeatCalculator singleSkillMaxSeatCalculator,
			IPersonSkillProvider personSkillProvider)
		{
			_stateHolder = stateHolder;
			_occupiedSeatCalculator = occupiedSeatCalculator;
			_nonBlendSkillCalculator = nonBlendSkillCalculator;
			_singleSkillDictionary = singleSkillDictionary;
    		_singleSkillMaxSeatCalculator = singleSkillMaxSeatCalculator;
			_personSkillProvider = personSkillProvider;
		}

		public void ResourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks)
		{
			resourceCalculateDate(localDate, useOccupancyAdjustment, considerShortBreaks, new List<IScheduleDay>(), new List<IScheduleDay>());
		}

		public void ResourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks, IList<IScheduleDay> toRemove, IList<IScheduleDay> toAdd)
		{
			resourceCalculateDate(localDate, useOccupancyAdjustment, considerShortBreaks, toRemove, toAdd);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.DateOnly.ToShortDateString")]
		private void resourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks
			, IList<IScheduleDay> toRemove, IList<IScheduleDay> toAdd)
		{
			if (_stateHolder.TeamLeaderMode)
				return;

			if (_stateHolder.SkipResourceCalculation)
				return;

			using (PerformanceOutput.ForOperation("ResourceCalculate " + localDate.ToShortDateString()))
			{
				var relevantProjections = ResourceCalculationContext.Container(_personSkillProvider);

				var useSingleSkillCalculations = UseSingleSkillCalculations(toRemove, toAdd);

				if (!useSingleSkillCalculations && !ResourceCalculationContext.InContext)
				{
					var extractor = new ScheduleProjectionExtractor(_personSkillProvider, _stateHolder.Skills.Min(s => s.DefaultResolution));
					relevantProjections = extractor.CreateRelevantProjectionList(_stateHolder.Schedules,
					                                                             TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
						                                                             localDate.AddDays(-1), localDate.AddDays(1)));
				}

				resourceCalculateDate(relevantProjections, localDate, useOccupancyAdjustment, considerShortBreaks, toRemove, toAdd);
			}
		}

		private static DateTimePeriod getPeriod(DateOnly localDate)
		{
			DateTime currentStart = localDate;
			DateTime currentEnd = currentStart.AddDays(1).AddTicks(-1);
			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentStart, currentEnd);
		}

		private void resourceCalculateDate(IResourceCalculationDataContainer relevantProjections,
		                                   DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks
		                                   , IList<IScheduleDay> toRemove, IList<IScheduleDay> toAdd)
		{
			var timePeriod = getPeriod(localDate);
			var ordinarySkills = new List<ISkill>();
			foreach (var skill in _stateHolder.Skills)
			{
				if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill &&
				    skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
					ordinarySkills.Add(skill);
			}

			var relevantSkillStaffPeriods =
				CreateSkillSkillStaffDictionaryOnSkills(_stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
				                                        ordinarySkills, timePeriod);

			var schedulingResultService = new SchedulingResultService(relevantSkillStaffPeriods, _stateHolder.Skills,
			                                                          relevantProjections, new SingleSkillCalculator(),
			                                                          useOccupancyAdjustment, _personSkillProvider);

			schedulingResultService.SchedulingResult(timePeriod, toRemove, toAdd);

			if (considerShortBreaks)
			{
				var periodDistributionService = new PeriodDistributionService(relevantProjections, 5);
				periodDistributionService.CalculateDay(relevantSkillStaffPeriods);
			}

			calculateMaxSeatsAndNonBlend(relevantProjections, localDate, toRemove, toAdd, timePeriod);
		}

		private void calculateMaxSeatsAndNonBlend(IResourceCalculationDataContainer relevantProjections, DateOnly localDate,
		                                          IList<IScheduleDay> toRemove, IList<IScheduleDay> toAdd, DateTimePeriod timePeriod)
		{
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods;
			List<ISkill> ordinarySkills = new List<ISkill>();
			foreach (var skill in _stateHolder.Skills)
			{
				if (skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
					ordinarySkills.Add(skill);
			}
			relevantSkillStaffPeriods =
				CreateSkillSkillStaffDictionaryOnSkills(_stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
				                                        ordinarySkills, timePeriod);

			if (toRemove.IsEmpty() && toAdd.IsEmpty())
			{
				_occupiedSeatCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods);
			}
			else
			{
				_singleSkillMaxSeatCalculator.Calculate(relevantSkillStaffPeriods, toRemove, toAdd);
			}

			ordinarySkills = new List<ISkill>();
			foreach (var skill in _stateHolder.Skills)
			{
				if (skill.SkillType.ForecastSource == ForecastSource.NonBlendSkill)
					ordinarySkills.Add(skill);
			}
			relevantSkillStaffPeriods =
				CreateSkillSkillStaffDictionaryOnSkills(_stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
				                                        ordinarySkills, timePeriod);
			_nonBlendSkillCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods, false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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

		public bool UseSingleSkillCalculations(IList<IScheduleDay> toRemove, IList<IScheduleDay> toAdd)
		{
			var useSingleSkillCalculations = toRemove.Count > 0 || toAdd.Count > 0;

			if (useSingleSkillCalculations)
				useSingleSkillCalculations = AllIsSingleSkilled(toRemove);

			if (useSingleSkillCalculations)
				useSingleSkillCalculations = AllIsSingleSkilled(toAdd);

			return useSingleSkillCalculations;
		}

		private bool AllIsSingleSkilled(IEnumerable<IScheduleDay> scheduleDays)
		{
			foreach (var scheduleDay in scheduleDays)
			{
				var person = scheduleDay.Person;
				var dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
				if (!_singleSkillDictionary.IsSingleSkill(person, dateOnly))
				{
					return false;
				}
			}

			return true;
		}
	}

	public class ResourceCalculationContext : IDisposable
	{
		[ThreadStatic] private static IResourceCalculationDataContainer _container;

		public static IResourceCalculationDataContainer Container(IPersonSkillProvider personSkillProvider)
		{
			return _container ?? new ResourceCalculationDataContainer(personSkillProvider);
		}

		public ResourceCalculationContext(IResourceCalculationDataContainer resources)
		{
			_container = resources;
		}

		public static bool InContext
		{
			get { return _container != null; }
		}

		public void Dispose()
		{
			_container = null;
		}
	}
}
