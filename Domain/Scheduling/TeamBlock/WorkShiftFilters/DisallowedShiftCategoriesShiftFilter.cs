using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class DisallowedShiftCategoriesShiftFilter
	{
        public IList<ShiftProjectionCache> Filter(HashSet<IShiftCategory> categories, IList<ShiftProjectionCache> shiftList)
        {
	        if (shiftList == null) return null;
		    if (shiftList.Count == 0)
				return shiftList;
			if (categories.Count == 0)
				return shiftList;
			var ret = shiftList.Where(shiftProjectionCache => !categories.Contains(shiftProjectionCache.TheWorkShift.ShiftCategory)).ToList();
			return ret;
		}
	}
}
