using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public class SwapAndModifyServiceNew : ISwapAndModifyServiceNew
	{
		private readonly ISwapServiceNew _swapService;
	    private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

	    public SwapAndModifyServiceNew(ISwapServiceNew swapService, IScheduleDayChangeCallback scheduleDayChangeCallback)
        {
			if(swapService == null)
				throw new ArgumentNullException("swapService");

            _swapService = swapService;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public IEnumerable<IBusinessRuleResponse> Swap(IPerson person1, IPerson person2, IList<DateOnly> dates, IList<DateOnly> lockedDates, IScheduleDictionary scheduleDictionary, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTagSetter scheduleTagSetter)
		{
			InParameter.NotNull("person1", person1);
			InParameter.NotNull("person2", person2);
			if(dates == null)
				throw new ArgumentNullException("dates");
			if (scheduleDictionary == null)
				throw new ArgumentNullException("scheduleDictionary");
			InParameter.ListCannotBeEmpty("dates", (IList)dates);
			if (person1 == person2)
				throw new ArgumentException("The Persons must be different");
			if(lockedDates == null)
				throw new ArgumentNullException("lockedDates");

		    var authorizationService = PrincipalAuthorization.Instance();
            var applicationFunction = DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment;
			var modifiedParts = new List<IScheduleDay>();
			
			foreach (var dateOnly in dates)
			{
                if (!authorizationService.IsPermitted(applicationFunction, dateOnly, person1) || !authorizationService.IsPermitted(applicationFunction, dateOnly, person2))
					throw new PermissionException("No permission to change the person assignment");

				var part1 = scheduleDictionary[person1].ScheduledDay(dateOnly);
				var part2 = scheduleDictionary[person2].ScheduledDay(dateOnly);

				if(lockedDates.Contains(dateOnly))
					continue;

				var returnedSwappedParts = swapParts(scheduleDictionary, part1, part2);

				modifiedParts.AddRange(returnedSwappedParts);

				foreach (var assignment in part1.PersonAssignmentCollection())
				{
					assignment.CheckRestrictions();
				}

				foreach (var assignment in part2.PersonAssignmentCollection())
				{
					assignment.CheckRestrictions();
				}
			}

			var responses = scheduleDictionary.Modify(ScheduleModifier.Scheduler, modifiedParts, newBusinessRuleCollection, _scheduleDayChangeCallback, scheduleTagSetter);

			return responses.Where(r => !r.Overridden).ToList();
		}

		private IEnumerable<IScheduleDay> swapParts(IScheduleDictionary scheduleDictionary, IScheduleDay part1, IScheduleDay part2)
		{
			return _swapService.Swap(part1, part2,scheduleDictionary);
		}
	}
}
