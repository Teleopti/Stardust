using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class ShiftCategoryRestrictionShiftFilter
	{
        public IList<ShiftProjectionCache> Filter(IShiftCategory category, IList<ShiftProjectionCache> shiftList)
        {
	        if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;

			if (category == null)
				return shiftList;
			var ret = shiftList.Where(shift => shift.TheWorkShift.ShiftCategory.Equals(category)).ToList();
			return ret;
		} 
	}
}
