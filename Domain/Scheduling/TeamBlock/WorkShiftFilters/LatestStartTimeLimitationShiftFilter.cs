using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface ILatestStartTimeLimitationShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, DateTime latestStart, IWorkShiftFinderResult finderResult);
	}

	public class LatestStartTimeLimitationShiftFilter : ILatestStartTimeLimitationShiftFilter
	{
        public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, DateTime latestStart, IWorkShiftFinderResult finderResult)
		{
			if (shiftList.Count == 0) return shiftList;
			int cntBefore = shiftList.Count;
			IList<IShiftProjectionCache> workShiftsWithinPeriod = new List<IShiftProjectionCache>();
			foreach (IShiftProjectionCache proj in shiftList)
			{
				var period = proj.MainShiftProjection.Period();
				if (!period.HasValue) continue;
				DateTimePeriod virtualPeriod = period.Value;
				if (virtualPeriod.StartDateTime <= latestStart)
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
