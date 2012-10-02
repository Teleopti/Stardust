using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IShiftCategoryFairnessSwapper
	{
		bool TrySwap(IShiftCategoryFairnessSwap suggestion, DateOnly dateOnly,
		             IList<IScheduleMatrixPro> matrixListForFairnessOptimization,
		             ISchedulePartModifyAndRollbackService rollbackService,
					 BackgroundWorker backgroundWorker);
	}

	public class ShiftCategoryFairnessSwapper: IShiftCategoryFairnessSwapper
	{
		private readonly ISwapServiceNew _swapService;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IShiftCategoryFairnessReScheduler _shiftCategoryFairnessReScheduler;
		private readonly IShiftCategoryChecker _shiftCategoryChecker;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IScheduleDictionary _dic;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public ShiftCategoryFairnessSwapper(ISwapServiceNew swapService, ISchedulingResultStateHolder schedulingResultStateHolder, 
			IShiftCategoryFairnessReScheduler shiftCategoryFairnessReScheduler, IShiftCategoryChecker shiftCategoryChecker, IDeleteSchedulePartService deleteSchedulePartService)
		{
			_swapService = swapService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_shiftCategoryFairnessReScheduler = shiftCategoryFairnessReScheduler;
			_shiftCategoryChecker = shiftCategoryChecker;
			_deleteSchedulePartService = deleteSchedulePartService;
			_dic = _schedulingResultStateHolder.Schedules;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalMember"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "responses")]
		public bool TrySwap(IShiftCategoryFairnessSwap suggestion, DateOnly dateOnly, IList<IScheduleMatrixPro> matrixListForFairnessOptimization,
			ISchedulePartModifyAndRollbackService rollbackService, BackgroundWorker backgroundWorker)
		{
			// start with group with less members and if there are more in the other reschedule them
        	var groupOne = suggestion.Group1;
        	var catOne = suggestion.ShiftCategoryFromGroup1;
			var groupTwo = suggestion.Group2;
			var catTwo = suggestion.ShiftCategoryFromGroup2;
        	var swappedInGroupTwo = new List<IPerson>();
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
				if (!_shiftCategoryChecker.DayHasShiftCategory(day1, catOne))
					return false;
				var swapSucces = false;
				foreach (var originalMember in groupTwo.OriginalMembers)
				{
					if(swappedInGroupTwo.Contains(originalMember)) continue;
					var day2 = getScheduleForPersonOnDay(dateOnly, matrixListForFairnessOptimization, originalMember);
					if (_shiftCategoryChecker.DayHasShiftCategory(day2, catTwo))
					{
						var modifiedParts = _swapService.Swap(new List<IScheduleDay> { day1, day2 }, _dic);
						var responses = rollbackService.ModifyParts(modifiedParts);
						if (!responses.Any())
						{
							swapSucces = true;
							swappedInGroupTwo.Add(originalMember);
							break;
						}
					}
					
				}
				if (!swapSucces)
					return false;
			}
			if (swappedInGroupTwo.Count < groupTwo.OriginalMembers.Count)
			{
				var toBeDeleted = new List<IScheduleDay>();
				var toBeRescheduled = new List<IPerson>();
				foreach (var person in groupTwo.OriginalMembers)
				{
					if(!swappedInGroupTwo.Contains(person))
					{
						toBeDeleted.Add(getScheduleForPersonOnDay(dateOnly,matrixListForFairnessOptimization,person));
						toBeRescheduled.Add(person);
					}
				}
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


	}

	public interface IShiftCategoryChecker
	{
		bool DayHasShiftCategory(IScheduleDay day, IShiftCategory shiftCategory);
	}

	public class ShiftCategoryChecker : IShiftCategoryChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool DayHasShiftCategory(IScheduleDay day, IShiftCategory shiftCategory)
		{
			if (!day.SignificantPart().Equals(SchedulePartView.MainShift))
				return false;
			return day.PersonAssignmentCollection()[0].MainShift.ShiftCategory.Equals(shiftCategory);
		}
	}
}