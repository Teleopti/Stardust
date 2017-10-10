using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface ILatestStartTimeLimitationShiftFilter
	{
		IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, DateTime latestStart);
	}

	public class LatestStartTimeLimitationShiftFilter : ILatestStartTimeLimitationShiftFilter
	{
        public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, DateTime latestStart)
		{
			if (shiftList.Count == 0) return shiftList;

			IList<ShiftProjectionCache> workShiftsWithinPeriod =
				shiftList.Select(s => new { Period = s.MainShiftProjection.Period(), s })
					.Where(s => s.Period.HasValue && s.Period.Value.StartDateTime <= latestStart)
					.Select(s => s.s)
					.ToList();

			return workShiftsWithinPeriod;
		}
	}
}
