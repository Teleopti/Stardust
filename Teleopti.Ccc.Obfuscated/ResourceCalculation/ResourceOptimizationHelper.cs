
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
		private readonly IPersonSkillProvider _personSkillProvider;

		public ResourceOptimizationHelper(ISchedulingResultStateHolder stateHolder,
			IOccupiedSeatCalculator occupiedSeatCalculator,
			INonBlendSkillCalculator nonBlendSkillCalculator,
			IPersonSkillProvider personSkillProvider)
		{
			_stateHolder = stateHolder;
			_occupiedSeatCalculator = occupiedSeatCalculator;
			_nonBlendSkillCalculator = nonBlendSkillCalculator;
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
			, IEnumerable<IScheduleDay> toRemove, IEnumerable<IScheduleDay> toAdd)
		{
			if (_stateHolder.TeamLeaderMode)
				return;

			if (_stateHolder.SkipResourceCalculation)
				return;

			using (PerformanceOutput.ForOperation("ResourceCalculate " + localDate.ToShortDateString()))
			{
				var relevantProjections = ResourceCalculationContext.Container(_personSkillProvider);

				if (!ResourceCalculationContext.InContext)
				{
					var extractor = new ScheduleProjectionExtractor(_personSkillProvider, _stateHolder.Skills.Min(s => s.DefaultResolution));
					relevantProjections = extractor.CreateRelevantProjectionList(_stateHolder.Schedules,
					                                                             TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
						                                                             localDate.AddDays(-1), localDate.AddDays(1)));
				}

				addAndRemoveScheduleDays(relevantProjections, toRemove, toAdd);

				resourceCalculateDate(relevantProjections, localDate, useOccupancyAdjustment, considerShortBreaks);
			}
		}

		private void addAndRemoveScheduleDays(IResourceCalculationDataContainer relevantProjections, IEnumerable<IScheduleDay> toRemove, IEnumerable<IScheduleDay> toAdd)
		{
			foreach (var scheduleDay in toRemove)
			{
				relevantProjections.RemoveScheduleDayFromContainer(scheduleDay,relevantProjections.MinSkillResolution);
			}
			foreach (var scheduleDay in toAdd)
			{
				relevantProjections.AddScheduleDayToContainer(scheduleDay, relevantProjections.MinSkillResolution);
			}
		}

		private static DateTimePeriod getPeriod(DateOnly localDate)
		{
			DateTime currentStart = localDate;
			DateTime currentEnd = currentStart.AddDays(1).AddTicks(-1);
			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentStart, currentEnd);
		}

		private void resourceCalculateDate(IResourceCalculationDataContainer relevantProjections,
		                                   DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks)
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

			schedulingResultService.SchedulingResult(timePeriod);

			if (considerShortBreaks)
			{
				var periodDistributionService = new PeriodDistributionService(relevantProjections, 5);
				periodDistributionService.CalculateDay(relevantSkillStaffPeriods);
			}

			calculateMaxSeatsAndNonBlend(relevantProjections, localDate, timePeriod);
		}

		private void calculateMaxSeatsAndNonBlend(IResourceCalculationDataContainer relevantProjections, DateOnly localDate, DateTimePeriod timePeriod)
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

				_occupiedSeatCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods);
			
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
