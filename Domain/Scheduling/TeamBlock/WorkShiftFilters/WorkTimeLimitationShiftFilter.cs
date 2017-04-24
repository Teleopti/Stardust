using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IWorkTimeLimitationShiftFilter
	{
		IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult);
	}

	public class WorkTimeLimitationShiftFilter : IWorkTimeLimitationShiftFilter
	{
		private readonly IUserCulture _userCulture;

		public WorkTimeLimitationShiftFilter(IUserCulture userCulture)
		{
			_userCulture = userCulture;
		}

		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (restriction == null) return null;
			if (finderResult == null) return null;
		    if (shiftList.Count == 0) return shiftList;
			if (!restriction.WorkTimeLimitation.EndTime.HasValue && !restriction.WorkTimeLimitation.StartTime.HasValue)
			{
				return shiftList;
			}

			var workShiftsWithinMinMax =
				shiftList.Where(
					s => restriction.WorkTimeLimitation.IsCorrespondingToWorkTimeLimitation(s.WorkShiftProjectionWorkTime)).ToList();
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(
					string.Format(_userCulture.GetCulture(), UserTexts.Resources.FilterOnWorkTimeLimitationsWithParams,
						restriction.WorkTimeLimitation.StartTimeString, restriction.WorkTimeLimitation.EndTimeString),
					shiftList.Count, workShiftsWithinMinMax.Count));
			return workShiftsWithinMinMax;
		}
	}
}
