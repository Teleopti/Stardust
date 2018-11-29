using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{ 
	public class BusinessRulesShiftFilter
	{
		private readonly ValidDateTimePeriodShiftFilter _validDateTimePeriodShiftFilter;
		private readonly ILongestPeriodForAssignmentCalculator _longestPeriodForAssignmentCalculator;

		public BusinessRulesShiftFilter(ValidDateTimePeriodShiftFilter validDateTimePeriodShiftFilter,
		                                ILongestPeriodForAssignmentCalculator longestPeriodForAssignmentCalculator)
		{
			_validDateTimePeriodShiftFilter = validDateTimePeriodShiftFilter;
			_longestPeriodForAssignmentCalculator = longestPeriodForAssignmentCalculator;
		}

		public IList<ShiftProjectionCache> Filter(IScheduleDictionary schedules, IPerson person, IList<ShiftProjectionCache> shiftList, DateOnly dateToCheck)
		{
			if (person == null) return null;
			if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;

			var approximateTime = dateToCheck.Date.AddHours(12);
			var approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime, person.PermissionInformation.DefaultTimeZone());
			DateTimePeriod? returnPeriod = new DateTimePeriod(approxUtc.AddDays(-2), approxUtc.AddDays(2));

			var scheduleRange = schedules[person];
			var newRulePeriod = _longestPeriodForAssignmentCalculator.PossiblePeriod(scheduleRange, dateToCheck);
			if (!newRulePeriod.HasValue)
			{
				return filterResults();
			}
			returnPeriod = returnPeriod.Value.Intersection(newRulePeriod.Value);
			if (!returnPeriod.HasValue)
			{
				return filterResults();
			}

			return _validDateTimePeriodShiftFilter.Filter(shiftList, returnPeriod.Value);
		}

		private static IList<ShiftProjectionCache> filterResults()
		{
			return new List<ShiftProjectionCache>();
		}
	}
}
