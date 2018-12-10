using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class ValidDateTimePeriodShiftFilter
	{
		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, DateTimePeriod validPeriod)
		{
			if (shiftList == null) return null;
		    if (shiftList.Count == 0) return shiftList;
			IList<ShiftProjectionCache> workShiftsWithinPeriod =
				shiftList.Select(s => new {s, OuterPeriod = s.TheMainShift.LayerCollection.OuterPeriod()})
					.Where(s => s.OuterPeriod.HasValue && validPeriod.Contains(s.OuterPeriod.Value))
					.Select(s => s.s)
					.ToList();

			return workShiftsWithinPeriod;
		}
	}
}
