using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IWorkTimeLimitationShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult);
	}

	public class WorkTimeLimitationShiftFilter : IWorkTimeLimitationShiftFilter
	{
		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (restriction == null) return null;
			if (finderResult == null) return null;
		    if (shiftList.Count == 0) return shiftList;
			IList<IShiftProjectionCache> workShiftsWithinMinMax = new List<IShiftProjectionCache>();
			if (restriction.WorkTimeLimitation.EndTime.HasValue || restriction.WorkTimeLimitation.StartTime.HasValue)
			{
				foreach (ShiftProjectionCache proj in shiftList)
				{
					TimeSpan contractTime = proj.WorkShiftProjectionContractTime;
					if (restriction.WorkTimeLimitation.IsCorrespondingToWorkTimeLimitation(contractTime))
						workShiftsWithinMinMax.Add(proj);
				}
				finderResult.AddFilterResults(
				new WorkShiftFilterResult(string.Format(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture, UserTexts.Resources.FilterOnWorkTimeLimitationsWithParams, restriction.WorkTimeLimitation.StartTimeString, restriction.WorkTimeLimitation.EndTimeString),
										  shiftList.Count, workShiftsWithinMinMax.Count));
			}
			else
			{
				return shiftList;
			}
			return workShiftsWithinMinMax;
		}
	}
}
