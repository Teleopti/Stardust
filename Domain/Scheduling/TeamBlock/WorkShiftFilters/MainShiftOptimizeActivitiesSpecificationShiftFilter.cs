using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IMainShiftOptimizeActivitiesSpecificationShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, ISpecification<IEditableShift> mainShiftActivitiesOptimizeSpecification);
	}

	public class MainShiftOptimizeActivitiesSpecificationShiftFilter : IMainShiftOptimizeActivitiesSpecificationShiftFilter
	{
		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, ISpecification<IEditableShift> mainShiftActivitiesOptimizeSpecification)
		{
			if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;
			IList<IShiftProjectionCache> ret = new List<IShiftProjectionCache>();
			foreach (var shiftProjectionCache in shiftList)
			{
				if (mainShiftActivitiesOptimizeSpecification != null && mainShiftActivitiesOptimizeSpecification.IsSatisfiedBy(shiftProjectionCache.TheMainShift))
					ret.Add(shiftProjectionCache);
			}
			return ret;
		}
	}
}
