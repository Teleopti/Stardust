using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SwapAndModifyService : ISwapAndModifyService
    {
        private readonly ISwapService _swapService;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

        public SwapAndModifyService(ISwapService swapService, IScheduleDayChangeCallback scheduleDayChangeCallback)
        {
            _swapService = swapService;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
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

        public IEnumerable<IBusinessRuleResponse> Swap(IPerson person1, IPerson person2, IList<DateOnly> dates,
            IScheduleDictionary scheduleDictionary, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTagSetter scheduleTagSetter)
        {
            InParameter.NotNull(nameof(person1), person1);
            InParameter.NotNull(nameof(person2), person2);
            InParameter.NotNull(nameof(dates), dates);
            InParameter.NotNull(nameof(scheduleDictionary), scheduleDictionary);
            InParameter.ListCannotBeEmpty(nameof(dates), dates);
            if (person1 == person2)
                throw new ArgumentException("The Persons must be different");

            var authorizationService = PrincipalAuthorization.Current();
            var applicationFunction = DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment;

            // a list to hold all modified parts
            List<IScheduleDay> modifiedParts = new List<IScheduleDay>();
            // for every day, take each persons schedule and send to swapservice and swap
            //IScheduleDictionary scheduleDictionary = schedulingResultStateHolder.Schedules;
            foreach (DateOnly dateOnly in dates)
            {
                if (!authorizationService.IsPermitted(applicationFunction, dateOnly, person1)
                    || !authorizationService.IsPermitted(applicationFunction, dateOnly, person2))
                    throw new PermissionException("No permission to change the person assignment");

                IScheduleDay part1 = scheduleDictionary[person1].ScheduledDay(dateOnly);
                IScheduleDay part2 = scheduleDictionary[person2].ScheduledDay(dateOnly);
                IList<IScheduleDay> selectedSchedules = new List<IScheduleDay> {part1, part2};
                modifiedParts.AddRange(swapParts(scheduleDictionary, selectedSchedules, false));

	            var ass1 = part1.PersonAssignment();
	            ass1?.CheckRestrictions();

	            var ass2 = part1.PersonAssignment();
	            ass2?.CheckRestrictions();
            }

            IEnumerable<IBusinessRuleResponse> responses =
                scheduleDictionary.Modify(ScheduleModifier.Scheduler, modifiedParts, newBusinessRuleCollection, _scheduleDayChangeCallback, scheduleTagSetter);

            return responses.Where(r => !r.Overridden).ToList();
        }

        private IEnumerable<IScheduleDay> swapParts(IScheduleDictionary scheduleDictionary, IList<IScheduleDay> selectedSchedules, bool ignoreAssignmentPermission)
        {
            _swapService.Init(selectedSchedules);
            return _swapService.SwapAssignments(scheduleDictionary, ignoreAssignmentPermission);
        }
    }
}
