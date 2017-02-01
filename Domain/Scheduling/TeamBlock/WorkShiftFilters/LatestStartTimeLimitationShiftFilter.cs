using System;
using System.Collections.Generic;
using System.Linq;
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

			IList<IShiftProjectionCache> workShiftsWithinPeriod =
				shiftList.Select(s => new { Period = s.MainShiftProjection.Period(), s })
					.Where(s => s.Period.HasValue && s.Period.Value.StartDateTime <= latestStart)
					.Select(s => s.s)
					.ToList();
			
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOnMinEndTimeOnRestriction, " "),
										  cntBefore, workShiftsWithinPeriod.Count));

			return workShiftsWithinPeriod;
		}
	}
}
