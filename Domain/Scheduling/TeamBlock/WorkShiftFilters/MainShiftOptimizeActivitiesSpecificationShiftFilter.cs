using System.Collections.Generic;
using System.Linq;
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
			if (mainShiftActivitiesOptimizeSpecification == null) return new List<IShiftProjectionCache>();
			IList<IShiftProjectionCache> ret =
				shiftList.Where(s => mainShiftActivitiesOptimizeSpecification.IsSatisfiedBy(s.TheMainShift)).ToList();

			return ret;
		}
	}
}
