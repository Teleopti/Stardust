using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IShiftCategoryFairnessSwapper
	{
		bool TrySwap(IShiftCategoryFairnessSwap suggestion, DateOnly dateOnly, IList<IScheduleMatrixPro> matrixListForFairnessOptimization, SchedulePartModifyAndRollbackService rollbackService);
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
		public bool TrySwap(IShiftCategoryFairnessSwap suggestion, DateOnly dateOnly, IList<IScheduleMatrixPro> matrixListForFairnessOptimization, SchedulePartModifyAndRollbackService rollbackService1)
		{
			//_schedulingResultStateHolder.UseValidation = false;
			var rules = _schedulingResultStateHolder.GetRulesToRun();
			//smäller om färre i tvåan
			for (var i = 0; i < suggestion.Group1.OriginalMembers.Count; i++)
			{
				var day1 = getScheduleForPersonOnDay(dateOnly, matrixListForFairnessOptimization, suggestion.Group1.OriginalMembers[i]);
				if (!dayHasShiftCategory(day1, suggestion.ShiftCategoryFromGroup1))
					return false;
				var swapSucces = false;
				foreach (var originalMember in suggestion.Group2.OriginalMembers)
				{
					var day2 = getScheduleForPersonOnDay(dateOnly, matrixListForFairnessOptimization, suggestion.Group2.OriginalMembers[i]);
					if (dayHasShiftCategory(day2, suggestion.ShiftCategoryFromGroup2))
					{
						var modifiedParts = _swapService.Swap(new List<IScheduleDay> { day1, day2 }, _dic);
						var responses = _dic.Modify(ScheduleModifier.AutomaticScheduling, modifiedParts, rules,
												new EmptyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
						if(!responses.Any())
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