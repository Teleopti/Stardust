using System;
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
        public IEnumerable<IBusinessRuleResponse> Swap(IPerson person1, IPerson person2, IList<DateOnly> dates, IList<DateOnly> lockedDates, IScheduleDictionary scheduleDictionary, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTagSetter scheduleTagSetter, TrackedCommandInfo trackedCommandInfo = null)
		{
			InParameter.NotNull("person1", person1);
			InParameter.NotNull("person2", person2);
			if(dates == null)
				throw new ArgumentNullException("dates");
			if (scheduleDictionary == null)
				throw new ArgumentNullException("scheduleDictionary");
			InParameter.ListCannotBeEmpty("dates", dates);
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

				IList<IScheduleDay> selectedSchedules = new List<IScheduleDay> { part1, part2 };

				modifiedParts.AddRange(SwapParts(scheduleDictionary, selectedSchedules, trackedCommandInfo));

				var ass1 = part1.PersonAssignment();
				if (ass1 != null)
				{
					ass1.CheckRestrictions();
				}

				var ass2 = part1.PersonAssignment();
				if (ass2 != null)
				{
					ass2.CheckRestrictions();
				}
			}

			var responses = scheduleDictionary.Modify(ScheduleModifier.Scheduler, modifiedParts, newBusinessRuleCollection, _scheduleDayChangeCallback, scheduleTagSetter);

			return responses.Where(r => !r.Overridden).ToList();
		}

		private IEnumerable<IScheduleDay> SwapParts(IScheduleDictionary scheduleDictionary, IList<IScheduleDay> selectedSchedules, TrackedCommandInfo trackedCommandInfo)
		{
			_swapService.Init(selectedSchedules);
			return _swapService.Swap(scheduleDictionary, trackedCommandInfo);
		}
	}
}
