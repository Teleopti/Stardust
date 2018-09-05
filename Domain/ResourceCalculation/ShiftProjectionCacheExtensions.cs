using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class ShiftProjectionCacheExtensions
	{
		public static void ClearMainShiftProjectionCaches(this IEnumerable<ShiftProjectionCache> shiftProjectionCaches)
		{
			foreach (var shiftProjectionCache in shiftProjectionCaches)
			{
				shiftProjectionCache.ClearMainShiftProjectionCache();
			}
		}
		
		public static DateTimePeriod WorkShiftProjectionPeriod(this ShiftProjectionCache shiftProjectionCache) => shiftProjectionCache.TheWorkShift.Projection.Period().Value;
	}
}