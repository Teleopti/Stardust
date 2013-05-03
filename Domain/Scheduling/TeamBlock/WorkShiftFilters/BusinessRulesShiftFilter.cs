using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IBusinessRulesShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IGroupPerson groupPerson, IList<IShiftProjectionCache> shiftList, DateOnly dateToCheck,
											IWorkShiftFinderResult finderResult);
	}

	public class BusinessRulesShiftFilter : IBusinessRulesShiftFilter
	{
		private readonly ISchedulingResultStateHolder _resultStateHolder;
		private readonly IValidDateTimePeriodShiftFilter _validDateTimePeriodShiftFilter;
		private readonly ILongestPeriodForAssignmentCalculator _longestPeriodForAssignmentCalculator;

		public BusinessRulesShiftFilter(ISchedulingResultStateHolder resultStateHolder,
		                                IValidDateTimePeriodShiftFilter validDateTimePeriodShiftFilter,
		                                ILongestPeriodForAssignmentCalculator longestPeriodForAssignmentCalculator)
		{
			_resultStateHolder = resultStateHolder;
			_validDateTimePeriodShiftFilter = validDateTimePeriodShiftFilter;
			_longestPeriodForAssignmentCalculator = longestPeriodForAssignmentCalculator;
		}

		public IList<IShiftProjectionCache> Filter(IGroupPerson groupPerson, IList<IShiftProjectionCache> shiftList,
												   DateOnly dateToCheck, IWorkShiftFinderResult finderResult)
		{
			if (groupPerson == null) return null;
			if (shiftList == null) return null;
			if (finderResult == null) return null;
		    if (shiftList.Count == 0) return shiftList;
			
			DateTime approximateTime = new DateTime(dateToCheck.Year, dateToCheck.Month, dateToCheck.Day, 12, 0, 0, DateTimeKind.Unspecified);
            DateTime approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime,
                                                             groupPerson.GroupMembers.First().PermissionInformation.DefaultTimeZone());
            DateTimePeriod? returnPeriod = new DateTimePeriod(approxUtc.AddDays(-2), approxUtc.AddDays(2));
            
			foreach (var person in groupPerson.GroupMembers)
			{
				var scheduleRange = _resultStateHolder.Schedules[person];
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
