using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class DisallowedShiftProjectionCachesFilter
	{
		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> notAllowedShiftProjectionCaches, IList<ShiftProjectionCache> shiftProjectionCaches)
		{
			if (shiftProjectionCaches == null) return null;
			if (shiftProjectionCaches.Count == 0) return shiftProjectionCaches;
			if (notAllowedShiftProjectionCaches.Count == 0) return shiftProjectionCaches;

			var notAllowedHashes = notAllowedShiftProjectionCaches.Select(notAllowedShiftProjectionCache => notAllowedShiftProjectionCache.GetHashCode()).ToList();
			var result = shiftProjectionCaches.Where(shiftProjectionCache => !notAllowedHashes.Contains(shiftProjectionCache.GetHashCode())).ToList();

			return result;
		}
	}
}
