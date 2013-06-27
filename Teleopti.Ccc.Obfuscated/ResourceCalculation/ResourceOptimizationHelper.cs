
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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

		public ResourceOptimizationHelper(ISchedulingResultStateHolder stateHolder,
			IOccupiedSeatCalculator occupiedSeatCalculator,
			INonBlendSkillCalculator nonBlendSkillCalculator,
			ISingleSkillDictionary singleSkillDictionary,
			ISingleSkillMaxSeatCalculator singleSkillMaxSeatCalculator)
		{
			_stateHolder = stateHolder;
			_occupiedSeatCalculator = occupiedSeatCalculator;
			_nonBlendSkillCalculator = nonBlendSkillCalculator;
			_singleSkillDictionary = singleSkillDictionary;
    		_singleSkillMaxSeatCalculator = singleSkillMaxSeatCalculator;
		}

		public void ResourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks)
		{
			resourceCalculateDate(localDate, useOccupancyAdjustment, true, considerShortBreaks, new List<IScheduleDay>(), new List<IScheduleDay>());
		}

		public void ResourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks, IList<IScheduleDay> toRemove, IList<IScheduleDay> toAdd)
		{
			resourceCalculateDate(localDate, useOccupancyAdjustment, true, considerShortBreaks, toRemove, toAdd);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.DateOnly.ToShortDateString")]
		private void resourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool calculateMaxSeatsAndNonBlend, bool considerShortBreaks
			, IList<IScheduleDay> toRemove, IList<IScheduleDay> toAdd)
		{
			if (_stateHolder.TeamLeaderMode)
				return;

			if (_stateHolder.SkipResourceCalculation)
				return;

			using (PerformanceOutput.ForOperation("ResourceCalculate " + localDate.ToShortDateString()))
			{
				var extractor = new ScheduleProjectionExtractor();
				IList<IVisualLayerCollection> relevantProjections = new List<IVisualLayerCollection>();

				var useSingleSkillCalculations = UseSingleSkillCalculations(toRemove, toAdd);

				if (!useSingleSkillCalculations)
					relevantProjections = extractor.CreateRelevantProjectionWithScheduleList(_stateHolder.Schedules,
																						 TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localDate.AddDays(-1), localDate.AddDays(1)));

				IList<IVisualLayerCollection> addedVisualLayerCollections = new List<IVisualLayerCollection>();
				foreach (IScheduleDay addedSchedule in toAdd)
				{
					
					var removedPersonAssignment = addedSchedule.AssignmentHighZOrder();
					if (removedPersonAssignment == null)
						continue;
					IVisualLayerCollection collection = removedPersonAssignment.ProjectionService().CreateProjection();
					addedVisualLayerCollections.Add(collection);
				}

				IList<IVisualLayerCollection> removedVisualLayerCollections = new List<IVisualLayerCollection>();
				foreach (IScheduleDay removedSchedule in toRemove)
				{
				    var addedPersonAssignment = removedSchedule.AssignmentHighZOrder();
                    if (addedPersonAssignment == null) continue;
                    IVisualLayerCollection collection = addedPersonAssignment.ProjectionService().CreateProjection();
					removedVisualLayerCollections.Add(collection);
				}

				resourceCalculateDate(relevantProjections, localDate, useOccupancyAdjustment, calculateMaxSeatsAndNonBlend, considerShortBreaks, removedVisualLayerCollections, addedVisualLayerCollections);
			}

		}

		private static DateTimePeriod getPeriod(DateOnly localDate)
		{
			DateTime currentStart = localDate;
			DateTime currentEnd = currentStart.AddDays(1).AddTicks(-1);
			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentStart, currentEnd);
		}

		private void resourceCalculateDate(IList<IVisualLayerCollection> relevantProjections,
			DateOnly localDate, bool useOccupancyAdjustment, bool calculateMaxSeatsAndNonBlend, bool considerShortBreaks
			, IList<IVisualLayerCollection> toRemove, IList<IVisualLayerCollection> toAdd)
		{
			var timePeriod = getPeriod(localDate);
			var ordinarySkills = new List<ISkill>();
			foreach (var skill in _stateHolder.Skills)
			{
				if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill && skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
					ordinarySkills.Add(skill);
			}

			var relevantSkillStaffPeriods = CreateSkillSkillStaffDictionaryOnSkills(_stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, ordinarySkills, timePeriod);

			var schedulingResultService = new SchedulingResultService(relevantSkillStaffPeriods, _stateHolder.Skills, relevantProjections, new SingleSkillCalculator(), useOccupancyAdjustment, _singleSkillDictionary);

			schedulingResultService.SchedulingResult(timePeriod, toRemove, toAdd);

			if (considerShortBreaks)
			{
				var periodDistributionService = new PeriodDistributionService(relevantProjections, 5);
				periodDistributionService.CalculateDay(relevantSkillStaffPeriods);
			}

			if (calculateMaxSeatsAndNonBlend)
			{
				ordinarySkills = new List<ISkill>();
				foreach (var skill in _stateHolder.Skills)
				{
					if (skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
						ordinarySkills.Add(skill);
				}
				relevantSkillStaffPeriods = CreateSkillSkillStaffDictionaryOnSkills(_stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, ordinarySkills, timePeriod);

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
				relevantSkillStaffPeriods = CreateSkillSkillStaffDictionaryOnSkills(_stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, ordinarySkills, timePeriod);
				_nonBlendSkillCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods, false);
            }      
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
}
