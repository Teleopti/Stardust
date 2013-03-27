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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IList<IShiftProjectionCache> Filter(IShiftCategory category, IList<IShiftProjectionCache> shiftList,
		                                           IWorkShiftFinderResult finderResult)
		{
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
