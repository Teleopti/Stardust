using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class CommonActivityFilter
	{
		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction)
		{
			if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;
			if (effectiveRestriction == null) return shiftList;
			if (!(schedulingOptions.TeamSameActivity && schedulingOptions.UseTeam)) return shiftList;

			if (effectiveRestriction.CommonActivity == null) return shiftList;

			var retList = (from shift in shiftList
						   let visualLayerPeriodList =
				shift.MainShiftProjection()
					.Where(c => c.Payload.Id == schedulingOptions.CommonActivity.Id)
					.Select(c => c.Period)
					.ToHashSet()
				where effectiveRestriction.CommonActivity.Periods.All(visualLayerPeriodList.Contains)
				select shift).ToList();
			return retList;
		}
	}
}