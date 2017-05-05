using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IShiftCategoryRestrictionShiftFilter
	{
		IList<ShiftProjectionCache> Filter(IShiftCategory category, IList<ShiftProjectionCache> shiftList,
															WorkShiftFinderResult finderResult);
	}
	
	public class ShiftCategoryRestrictionShiftFilter : IShiftCategoryRestrictionShiftFilter
	{
        public IList<ShiftProjectionCache> Filter(IShiftCategory category, IList<ShiftProjectionCache> shiftList,
		                                           WorkShiftFinderResult finderResult)
        {
	        if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;

			if (category == null)
				return shiftList;
			var before = shiftList.Count;
			var ret = shiftList.Where(shift => shift.TheWorkShift.ShiftCategory.Equals(category)).ToList();
			finderResult.AddFilterResults(new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOn, " ") + category.Description, before, ret.Count));
			return ret;
		} 
	}
}
