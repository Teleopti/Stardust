using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SwapAndModifyService : ISwapAndModifyService
    {
        private readonly ISwapService _swapService;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public SwapAndModifyService(ISwapService swapService, IScheduleDayChangeCallback scheduleDayChangeCallback, ITimeZoneGuard timeZoneGuard)
        {
            _swapService = swapService;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
			_timeZoneGuard = timeZoneGuard;
		}
		
        public IEnumerable<IBusinessRuleResponse> SwapShiftTradeSwapDetails(ReadOnlyCollection<IShiftTradeSwapDetail> shiftTradeSwapDetails, IScheduleDictionary scheduleDictionary, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTagSetter scheduleTagSetter)
        {
            List<IScheduleDay> modifiedParts = new List<IScheduleDay>();
            foreach (IShiftTradeSwapDetail detail in shiftTradeSwapDetails)
            {
                IScheduleDay part1 = scheduleDictionary[detail.PersonFrom].ScheduledDay(detail.DateFrom);
                IScheduleDay part2 = scheduleDictionary[detail.PersonTo].ScheduledDay(detail.DateTo);
                IList<IScheduleDay> selectedSchedules = new List<IScheduleDay> { part1, part2 };
                modifiedParts.AddRange(swapParts(scheduleDictionary, selectedSchedules, true));
            }

			var ruleRepsonses = scheduleDictionary.Modify(ScheduleModifier.Scheduler, modifiedParts, newBusinessRuleCollection, _scheduleDayChangeCallback, scheduleTagSetter);

			//if rules on the day that are not overriden return them
			var ruleRepsonsesOnDay = new List<IBusinessRuleResponse>(ruleRepsonses)
				.FindAll(new BusinessRuleResponseContainsDateSpecification(shiftTradeSwapDetails).IsSatisfiedBy).Where(r => !r.Overridden).ToList();

			if (ruleRepsonsesOnDay.Count > 0)
				return ruleRepsonsesOnDay;

			// if no response on day just override them and try again
			ruleRepsonses.ToList().ForEach(newBusinessRuleCollection.DoNotHaltModify);

			ruleRepsonses = scheduleDictionary.Modify(ScheduleModifier.Scheduler, modifiedParts, newBusinessRuleCollection, _scheduleDayChangeCallback, scheduleTagSetter);
			
			return ruleRepsonses.Where(r => !r.Overridden).ToList();
        }

        private IEnumerable<IScheduleDay> swapParts(IScheduleDictionary scheduleDictionary, IList<IScheduleDay> selectedSchedules, bool ignoreAssignmentPermission)
        {
            _swapService.Init(selectedSchedules);
            return _swapService.SwapAssignments(scheduleDictionary, ignoreAssignmentPermission, _timeZoneGuard);
        }
    }
}
