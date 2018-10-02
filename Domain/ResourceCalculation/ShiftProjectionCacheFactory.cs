using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ShiftProjectionCacheFactory
	{
		public ShiftProjectionCache Create(IWorkShift workShift, IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = null)
		{
			return new ShiftProjectionCache(workShift, dateOnlyAsDateTimePeriod);
		}
	}
}