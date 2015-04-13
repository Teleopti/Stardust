using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IDisallowedShiftCategoriesShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftCategory> categories, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);
	}
	
	public class DisallowedShiftCategoriesShiftFilter : IDisallowedShiftCategoriesShiftFilter
	{
        public IList<IShiftProjectionCache> Filter(IList<IShiftCategory> categories, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
        {
	        if (shiftList == null) return null;
			if (finderResult == null) return null;
		    if (shiftList.Count == 0)
				return shiftList;
			if (categories.Count == 0)
				return shiftList;
			int before = shiftList.Count;
			var ret = shiftList.Where(shiftProjectionCache => !categories.Contains(shiftProjectionCache.TheWorkShift.ShiftCategory)).ToList();

			finderResult.AddFilterResults(new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOn, " ") + categories.Count + UserTexts.Resources.NotAllowedShiftCategories, before, ret.Count));
			return ret;
		}
	}
}
