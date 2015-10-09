using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IBusinessRulesShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IPerson person, IList<IShiftProjectionCache> shiftList, DateOnly dateToCheck,
											IWorkShiftFinderResult finderResult);
	}

	public class BusinessRulesShiftFilter : IBusinessRulesShiftFilter
	{
		private readonly Func<IScheduleRangeForPerson> _scheduleRangeForPerson;
		private readonly IValidDateTimePeriodShiftFilter _validDateTimePeriodShiftFilter;
		private readonly ILongestPeriodForAssignmentCalculator _longestPeriodForAssignmentCalculator;

		public BusinessRulesShiftFilter(Func<IScheduleRangeForPerson> scheduleRangeForPerson,
		                                IValidDateTimePeriodShiftFilter validDateTimePeriodShiftFilter,
		                                ILongestPeriodForAssignmentCalculator longestPeriodForAssignmentCalculator)
		{
			_scheduleRangeForPerson = scheduleRangeForPerson;
			_validDateTimePeriodShiftFilter = validDateTimePeriodShiftFilter;
			_longestPeriodForAssignmentCalculator = longestPeriodForAssignmentCalculator;
		}

		public IList<IShiftProjectionCache> Filter(IPerson person, IList<IShiftProjectionCache> shiftList,
		                                           DateOnly dateToCheck, IWorkShiftFinderResult finderResult)
		{
			if (person == null) return null;
			if (shiftList == null) return null;
			if (finderResult == null) return null;
			if (shiftList.Count == 0) return shiftList;

			var approximateTime = dateToCheck.Date.AddHours(12);
			var approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime, person.PermissionInformation.DefaultTimeZone());
			DateTimePeriod? returnPeriod = new DateTimePeriod(approxUtc.AddDays(-2), approxUtc.AddDays(2));

			var scheduleRange = _scheduleRangeForPerson().ForPerson(person);
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

		private static IList<IShiftProjectionCache> filterResults(IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
		{
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(UserTexts.Resources.CannotFindAValidPeriodAccordingToTheBusinessRules,
				                          shiftList.Count, 0));
			return new List<IShiftProjectionCache>();
		}
	}
}
