using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IShiftCategoryRestrictionShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IShiftCategory category, IList<IShiftProjectionCache> shiftList,
															IWorkShiftFinderResult finderResult);
	}
	
	public class ShiftCategoryRestrictionShiftFilter : IShiftCategoryRestrictionShiftFilter
	{
        public IList<IShiftProjectionCache> Filter(IShiftCategory category, IList<IShiftProjectionCache> shiftList,
		                                           IWorkShiftFinderResult finderResult)
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
