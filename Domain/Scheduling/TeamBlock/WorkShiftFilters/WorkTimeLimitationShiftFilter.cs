using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class WorkTimeLimitationShiftFilter
	{
		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction)
		{
			if (shiftList == null) return null;
			if (restriction == null) return null;
		    if (shiftList.Count == 0) return shiftList;
			if (!restriction.WorkTimeLimitation.EndTime.HasValue && !restriction.WorkTimeLimitation.StartTime.HasValue)
			{
				return shiftList;
			}

			var workShiftsWithinMinMax =
				shiftList.Where(
					s => restriction.WorkTimeLimitation.IsCorrespondingToWorkTimeLimitation(s.WorkShiftProjectionContractTime)).ToList();
			return workShiftsWithinMinMax;
		}
	}
}
