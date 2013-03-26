using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
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

		public IList<IShiftProjectionCache> Filter(IPerson person, IList<IShiftProjectionCache> shiftList,
												   DateOnly dateToCheck, IWorkShiftFinderResult finderResult)
		{
		    if (shiftList == null) throw new ArgumentNullException("shiftList");
		    if (finderResult == null) throw new ArgumentNullException("finderResult");
		    if (shiftList.Count == 0) return shiftList;
			var scheduleRange = _resultStateHolder.Schedules[person];
			var rulePeriod = _longestPeriodForAssignmentCalculator.PossiblePeriod(scheduleRange, dateToCheck);
			if (!rulePeriod.HasValue)
			{
				finderResult.AddFilterResults(
					new WorkShiftFilterResult(UserTexts.Resources.CannotFindAValidPeriodAccordingToTheBusinessRules,
											  shiftList.Count, 0));
				return new List<IShiftProjectionCache>();
			}

			return _validDateTimePeriodShiftFilter.Filter(shiftList, rulePeriod.Value, finderResult);
		}
	}
}
