using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IEarliestEndTimeLimitationShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, DateTime earliestEnd, IWorkShiftFinderResult finderResult);
	}

	public class EarliestEndTimeLimitationShiftFilter : IEarliestEndTimeLimitationShiftFilter
	{
        public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, DateTime earliestEnd, IWorkShiftFinderResult finderResult)
        {
	        if (shiftList == null) return null;
			if (finderResult == null) return null;
		    if (shiftList.Count == 0) return shiftList;
			int cntBefore = shiftList.Count;
			IList<IShiftProjectionCache> workShiftsWithinPeriod = new List<IShiftProjectionCache>();
			for (int workShiftCounter = 0; workShiftCounter < shiftList.Count; workShiftCounter++)
			{
				IShiftProjectionCache proj = shiftList[workShiftCounter];

				if (!proj.MainShiftProjection.Period().HasValue) continue;
				DateTimePeriod virtualPeriod = proj.MainShiftProjection.Period().Value;
				if (virtualPeriod.EndDateTime >= earliestEnd)
				{
					workShiftsWithinPeriod.Add(proj);
				}
			}
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOnMinEndTimeOnRestriction, " "),
										  cntBefore, workShiftsWithinPeriod.Count));

			return workShiftsWithinPeriod;
		}
	}
}
