using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
	public interface IShiftCategoryFairnessSwapper
	{
		bool TrySwap(IShiftCategoryFairnessSwap suggestion, DateOnly dateOnly, IList<IScheduleMatrixPro> matrixListForFairnessOptimization,
			ISchedulePartModifyAndRollbackService rollbackService, BackgroundWorker backgroundWorker, bool useAverageShiftLengths, IOptimizationPreferences optimizationPreferences);
	}

	public class ShiftCategoryFairnessSwapper: IShiftCategoryFairnessSwapper
	{
		private readonly ISwapServiceNew _swapService;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IShiftCategoryFairnessReScheduler _shiftCategoryFairnessReScheduler;
		private readonly IShiftCategoryChecker _shiftCategoryChecker;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IShiftCategoryFairnessPersonsSwappableChecker _swappableChecker;
		private readonly IShiftCategoryFairnessContractToleranceChecker _shiftCategoryFairnessContractToleranceChecker;
		private readonly IOptimizationOverLimitByRestrictionDeciderCreator _optimizationOverLimitByRestrictionDeciderCreator;
		private readonly IScheduleDictionary _dic;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public ShiftCategoryFairnessSwapper(ISwapServiceNew swapService, ISchedulingResultStateHolder schedulingResultStateHolder, 
			IShiftCategoryFairnessReScheduler shiftCategoryFairnessReScheduler, IShiftCategoryChecker shiftCategoryChecker, 
			IDeleteSchedulePartService deleteSchedulePartService, IShiftCategoryFairnessPersonsSwappableChecker swappableChecker,
			IShiftCategoryFairnessContractToleranceChecker shiftCategoryFairnessContractToleranceChecker,
			IOptimizationOverLimitByRestrictionDeciderCreator optimizationOverLimitByRestrictionDeciderCreator)
		{
			_swapService = swapService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_shiftCategoryFairnessReScheduler = shiftCategoryFairnessReScheduler;
			_shiftCategoryChecker = shiftCategoryChecker;
			_deleteSchedulePartService = deleteSchedulePartService;
			_swappableChecker = swappableChecker;
			_shiftCategoryFairnessContractToleranceChecker = shiftCategoryFairnessContractToleranceChecker;
			_optimizationOverLimitByRestrictionDeciderCreator = optimizationOverLimitByRestrictionDeciderCreator;
			_dic = _schedulingResultStateHolder.Schedules;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalMember"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "responses")]
		public bool TrySwap(IShiftCategoryFairnessSwap suggestion, DateOnly dateOnly, IList<IScheduleMatrixPro> matrixListForFairnessOptimization,
			ISchedulePartModifyAndRollbackService rollbackService, BackgroundWorker backgroundWorker, bool useAverageShiftLengths, IOptimizationPreferences optimizationPreferences)
		{
			// start with group with less members and if there are more in the other reschedule them
        	var groupOne = suggestion.Group1;
        	var catOne = suggestion.ShiftCategoryFromGroup1;
			var groupTwo = suggestion.Group2;
			var catTwo = suggestion.ShiftCategoryFromGroup2;
        	var swappedInGroupTwo = new List<IPerson>();
        	var swappedInGroupOne = new List<IPerson>();
			var result = true;
			if(groupOne.OriginalMembers.Count > groupTwo.OriginalMembers.Count)
			{
				groupOne = suggestion.Group2;
				catOne = suggestion.ShiftCategoryFromGroup2;
				groupTwo = suggestion.Group1;
				catTwo = suggestion.ShiftCategoryFromGroup1;
			}
			
			foreach (var groupOneMember in groupOne.OriginalMembers)
			{
				var day1 = getScheduleForPersonOnDay(dateOnly, matrixListForFairnessOptimization, groupOneMember);
				if(_shiftCategoryChecker.DayHasDayOff(day1)) continue;
				if (!_shiftCategoryChecker.DayHasShiftCategory(day1, catOne))
					return false;
				var foundSwap = false;
				foreach (var groupTwoMember in groupTwo.OriginalMembers)
				{
					if(swappedInGroupTwo.Contains(groupTwoMember)) continue;
                    var day2 = getScheduleForPersonOnDay(dateOnly, matrixListForFairnessOptimization, groupTwoMember);
					if (_shiftCategoryChecker.DayHasDayOff(day2)) continue;
					var scheduleDays = new List<IScheduleDay>{day1, day2};
                    if(!_swappableChecker.PersonsAreSwappable(groupOneMember, groupTwoMember, dateOnly, scheduleDays, useAverageShiftLengths)) 
                        continue;
					
					if (_shiftCategoryChecker.DayHasShiftCategory(day2, catTwo))
					{
						var modifiedParts = _swapService.Swap(day1, day2, _dic);
						var responses = rollbackService.ModifyParts(modifiedParts);
						if (responses.Any())
							return false;
						if(!useAverageShiftLengths)
						{
							if (_shiftCategoryFairnessContractToleranceChecker.IsOutsideTolerance(matrixListForFairnessOptimization, groupOneMember)
							|| _shiftCategoryFairnessContractToleranceChecker.IsOutsideTolerance(matrixListForFairnessOptimization, groupTwoMember))
								return false;
						}
						
						var matrix =  getMatrixForPersonOnDay(dateOnly, matrixListForFairnessOptimization, groupOneMember);
						var restrictionCheck = _optimizationOverLimitByRestrictionDeciderCreator.GetDecider(dateOnly, matrix,
						                                                                                    optimizationPreferences);
						if (restrictionCheck.OverLimit().Any()) 
							return false;

						matrix = getMatrixForPersonOnDay(dateOnly, matrixListForFairnessOptimization, groupTwoMember);
						restrictionCheck = _optimizationOverLimitByRestrictionDeciderCreator.GetDecider(dateOnly, matrix,
																											optimizationPreferences);
						if (restrictionCheck.OverLimit().Any()) return false;

						swappedInGroupTwo.Add(groupTwoMember);
						swappedInGroupOne.Add(groupOneMember);
						foundSwap = true;
						break;
					}
				}
				if (!foundSwap) return false;
			}
			// if all day offs for example
			if (!swappedInGroupOne.Any()) return false;

			if (swappedInGroupTwo.Count < groupTwo.OriginalMembers.Count)
			{
				var toBeDeleted = new List<IScheduleDay>();
				var toBeRescheduled = new List<IPerson>();
				foreach (var person in groupTwo.OriginalMembers)
				{
					if(!swappedInGroupTwo.Contains(person))
					{
						var day = getScheduleForPersonOnDay(dateOnly, matrixListForFairnessOptimization, person);
						if(!_shiftCategoryChecker.DayHasDayOff(day))
						{
							toBeDeleted.Add(day);
							toBeRescheduled.Add(person);
						}
					}
				}
				if(toBeDeleted.Count > 0)
					_deleteSchedulePartService.Delete(toBeDeleted, new DeleteOption {MainShift = true}, rollbackService,
				                                  backgroundWorker);
				result = _shiftCategoryFairnessReScheduler.Execute(toBeRescheduled, dateOnly, matrixListForFairnessOptimization);
			}

			return result;
		}

		private static  IScheduleDay getScheduleForPersonOnDay(DateOnly dateOnly, IEnumerable<IScheduleMatrixPro> matrixListForFairnessOptimization, IPerson person)
		{
			IScheduleDay day = null;
			foreach (var scheduleMatrixPro in matrixListForFairnessOptimization)
			{
				if(scheduleMatrixPro.Person.Equals(person))
				{
					var tmpDay = scheduleMatrixPro.GetScheduleDayByKey(dateOnly);
					if(tmpDay != null && scheduleMatrixPro.UnlockedDays.Contains(tmpDay))
					{
						day = tmpDay.DaySchedulePart();
						break;
					}
				}
			}
			return day;
		}

		private static IScheduleMatrixPro getMatrixForPersonOnDay(DateOnly dateOnly, IEnumerable<IScheduleMatrixPro> matrixListForFairnessOptimization, IPerson person)
		{
			return (from scheduleMatrixPro in matrixListForFairnessOptimization where scheduleMatrixPro.Person.Equals(person) let tmpDay = scheduleMatrixPro.GetScheduleDayByKey(dateOnly) where tmpDay != null && scheduleMatrixPro.UnlockedDays.Contains(tmpDay) select scheduleMatrixPro).FirstOrDefault();
		}
	}

	public interface IShiftCategoryChecker
	{
		bool DayHasShiftCategory(IScheduleDay day, IShiftCategory shiftCategory);
		bool DayHasDayOff(IScheduleDay day);
	}

	public class ShiftCategoryChecker : IShiftCategoryChecker
	{
		public bool DayHasShiftCategory(IScheduleDay day, IShiftCategory shiftCategory)
		{
			if (day == null) return false;
			if (!day.SignificantPart().Equals(SchedulePartView.MainShift))
				return false;
			return day.PersonAssignmentCollection()[0].MainShift.ShiftCategory.Equals(shiftCategory);
		}

		public bool DayHasDayOff(IScheduleDay day)
		{
			if (day == null) return true;
			return day.SignificantPart().Equals(SchedulePartView.DayOff);
		}
	}
}