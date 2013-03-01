using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters
{
	public interface IMainShiftOptimizeActivitiesSpecificationShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, ISpecification<IMainShift> mainShiftActivitiesOptimizeSpecification);
	}
	
	public class MainShiftOptimizeActivitiesSpecificationShiftFilter : IMainShiftOptimizeActivitiesSpecificationShiftFilter
	{
		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, ISpecification<IMainShift> mainShiftActivitiesOptimizeSpecification)
		{
			IList<IShiftProjectionCache> ret = new List<IShiftProjectionCache>();
			if (shiftList != null)
				foreach (var shiftProjectionCache in shiftList)
				{
					if (mainShiftActivitiesOptimizeSpecification != null && mainShiftActivitiesOptimizeSpecification.IsSatisfiedBy(shiftProjectionCache.TheMainShift))
						ret.Add(shiftProjectionCache);
				}

			return ret;
		} 
	}
}
