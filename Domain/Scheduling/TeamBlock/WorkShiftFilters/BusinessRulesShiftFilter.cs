using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{ 
	public class BusinessRulesShiftFilter
	{
		private readonly IValidDateTimePeriodShiftFilter _validDateTimePeriodShiftFilter;
		private readonly ILongestPeriodForAssignmentCalculator _longestPeriodForAssignmentCalculator;

		public BusinessRulesShiftFilter(IValidDateTimePeriodShiftFilter validDateTimePeriodShiftFilter,
		                                ILongestPeriodForAssignmentCalculator longestPeriodForAssignmentCalculator)
		{
			_validDateTimePeriodShiftFilter = validDateTimePeriodShiftFilter;
			_longestPeriodForAssignmentCalculator = longestPeriodForAssignmentCalculator;
		}

		public IList<ShiftProjectionCache> Filter(IScheduleDictionary schedules, IPerson person, IList<ShiftProjectionCache> shiftList,
		                                           DateOnly dateToCheck, IWorkShiftFinderResult finderResult)
		{
			if (person == null) return null;
			if (shiftList == null) return null;
			if (finderResult == null) return null;
			if (shiftList.Count == 0) return shiftList;

			var approximateTime = dateToCheck.Date.AddHours(12);
			var approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime, person.PermissionInformation.DefaultTimeZone());
			DateTimePeriod? returnPeriod = new DateTimePeriod(approxUtc.AddDays(-2), approxUtc.AddDays(2));

			var scheduleRange = schedules[person];
			var newRulePeriod = _longestPeriodForAssignmentCalculator.PossiblePeriod(scheduleRange, dateToCheck);
			if (!newRulePeriod.HasValue)
			{
				return filterResults(shiftList, finderResult);
			}
			returnPeriod = returnPeriod.Value.Intersection(newRulePeriod.Value);
			if (!returnPeriod.HasValue)
			{
				return filterResults(shiftList, finderResult);
			}

			return _validDateTimePeriodShiftFilter.Filter(shiftList, returnPeriod.Value, finderResult);
		}

		private static IList<ShiftProjectionCache> filterResults(IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
		{
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(UserTexts.Resources.CannotFindAValidPeriodAccordingToTheBusinessRules,
				                          shiftList.Count, 0));
			return new List<ShiftProjectionCache>();
		}
	}
}
