using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IMainShiftOptimizeActivitiesSpecificationShiftFilter
	{
		IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, ISpecification<IEditableShift> mainShiftActivitiesOptimizeSpecification);
	}

	public class MainShiftOptimizeActivitiesSpecificationShiftFilter : IMainShiftOptimizeActivitiesSpecificationShiftFilter
	{
		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, ISpecification<IEditableShift> mainShiftActivitiesOptimizeSpecification)
		{
			if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;
			if (mainShiftActivitiesOptimizeSpecification == null) return new List<ShiftProjectionCache>();
			IList<ShiftProjectionCache> ret =
				shiftList.Where(s => mainShiftActivitiesOptimizeSpecification.IsSatisfiedBy(s.TheMainShift)).ToList();

			return ret;
		}
	}
}
