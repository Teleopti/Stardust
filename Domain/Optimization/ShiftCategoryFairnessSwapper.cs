using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IShiftCategoryFairnessSwapper
	{
		bool TrySwap(IShiftCategoryFairnessSwap suggestion, DateOnly dateOnly,
		             IList<IScheduleMatrixPro> matrixListForFairnessOptimization,
		             ISchedulePartModifyAndRollbackService rollbackService);
	}

	public class ShiftCategoryFairnessSwapper: IShiftCategoryFairnessSwapper
	{
		private readonly ISwapServiceNew _swapService;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IScheduleDictionary _dic;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public ShiftCategoryFairnessSwapper(ISwapServiceNew swapService, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_swapService = swapService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_dic = _schedulingResultStateHolder.Schedules;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalMember"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "responses")]
		public bool TrySwap(IShiftCategoryFairnessSwap suggestion, DateOnly dateOnly, IList<IScheduleMatrixPro> matrixListForFairnessOptimization,
			ISchedulePartModifyAndRollbackService rollbackService)
		{
			// start with group with less members and if there are more in the other reschedule them
        	var groupOne = suggestion.Group1;
        	var catOne = suggestion.ShiftCategoryFromGroup1;
			var groupTwo = suggestion.Group2;
			var catTwo = suggestion.ShiftCategoryFromGroup2;

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
				if (!dayHasShiftCategory(day1, catOne))
					return false;
				var swapSucces = false;
				foreach (var originalMember in groupTwo.OriginalMembers)
				{
					var day2 = getScheduleForPersonOnDay(dateOnly, matrixListForFairnessOptimization, originalMember);
					if (dayHasShiftCategory(day2, catTwo))
					{
						var modifiedParts = _swapService.Swap(new List<IScheduleDay> { day1, day2 }, _dic);
						var responses = rollbackService.ModifyParts(modifiedParts);
						if (!responses.Any())
						{
							swapSucces = true;
							break;
						}
					}
					
				}
				if (!swapSucces)
					return false;
			}
			
			return true;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private bool dayHasShiftCategory(IScheduleDay day, IShiftCategory shiftCategory)
		{
			if (!day.SignificantPart().Equals(SchedulePartView.MainShift))
				return false;
			return day.PersonAssignmentCollection()[0].MainShift.ShiftCategory.Equals(shiftCategory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		private IScheduleDay getScheduleForPersonOnDay(DateOnly dateOnly, IEnumerable<IScheduleMatrixPro> matrixListForFairnessOptimization, IPerson person)
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

}