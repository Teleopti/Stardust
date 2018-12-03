using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SwapAndModifyServiceNew : ISwapAndModifyServiceNew
	{
		private readonly ISwapServiceNew _swapService;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IPersistableScheduleDataPermissionChecker _permissionChecker;

		public SwapAndModifyServiceNew(ISwapServiceNew swapService, IScheduleDayChangeCallback scheduleDayChangeCallback, IPersistableScheduleDataPermissionChecker permissionChecker)
		{
			if(swapService == null)
				throw new ArgumentNullException(nameof(swapService));

			_swapService = swapService;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_permissionChecker = permissionChecker;
		}
		
		public IEnumerable<IBusinessRuleResponse> Swap(IPerson person1, IPerson person2, IList<DateOnly> dates, IList<DateOnly> lockedDates, IScheduleDictionary scheduleDictionary, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTagSetter scheduleTagSetter, TrackedCommandInfo trackedCommandInfo = null)
		{
			InParameter.NotNull(nameof(person1), person1);
			InParameter.NotNull(nameof(person2), person2);
			if(dates == null)
				throw new ArgumentNullException(nameof(dates));
			if (scheduleDictionary == null)
				throw new ArgumentNullException(nameof(scheduleDictionary));
			InParameter.ListCannotBeEmpty(nameof(dates), dates);
			if (person1 == person2)
				throw new ArgumentException("The Persons must be different");
			if(lockedDates == null)
				throw new ArgumentNullException(nameof(lockedDates));

			var modifiedParts = new List<IScheduleDay>();
			if (dates.Any(dateOnly => !_permissionChecker.IsModifyPersonAssPermitted(dateOnly, person1) || !_permissionChecker.IsModifyPersonAssPermitted(dateOnly, person2)))
			{
				throw new PermissionException("No permission to change the person assignment");
			}
			
			foreach (var dateOnly in dates)
			{
				var part1 = scheduleDictionary[person1].ScheduledDay(dateOnly);
				var part2 = scheduleDictionary[person2].ScheduledDay(dateOnly);


				if(lockedDates.Contains(dateOnly))
					continue;

				IList<IScheduleDay> selectedSchedules = new List<IScheduleDay> { part1, part2 };

				modifiedParts.AddRange(SwapParts(scheduleDictionary, selectedSchedules, trackedCommandInfo));

				var ass1 = part1.PersonAssignment();
				ass1?.CheckRestrictions();

				var ass2 = part1.PersonAssignment();
				ass2?.CheckRestrictions();
			}

			var responses = scheduleDictionary.Modify(ScheduleModifier.Scheduler, modifiedParts, newBusinessRuleCollection, _scheduleDayChangeCallback, scheduleTagSetter);

			return responses.Where(r => !r.Overridden).ToList();
		}

		private IEnumerable<IScheduleDay> SwapParts(IScheduleDictionary scheduleDictionary, IList<IScheduleDay> selectedSchedules, TrackedCommandInfo trackedCommandInfo)
		{
			return _swapService.Swap(scheduleDictionary, selectedSchedules, trackedCommandInfo);
		}
	}
}
