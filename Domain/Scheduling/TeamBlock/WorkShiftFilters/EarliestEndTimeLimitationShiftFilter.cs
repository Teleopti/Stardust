using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IEarliestEndTimeLimitationShiftFilter
	{
		IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, DateTime earliestEnd);
	}

	public class EarliestEndTimeLimitationShiftFilter : IEarliestEndTimeLimitationShiftFilter
	{
        public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, DateTime earliestEnd)
        {
	        if (shiftList == null) return null;
		    if (shiftList.Count == 0) return shiftList;
	        IList<ShiftProjectionCache> workShiftsWithinPeriod =
		        shiftList.Select(s => new {Period = s.MainShiftProjection.Period(), s})
			        .Where(s => s.Period.HasValue && s.Period.Value.EndDateTime >= earliestEnd)
			        .Select(s => s.s)
			        .ToList();

			return workShiftsWithinPeriod;
		}
	}
}
