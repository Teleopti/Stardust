using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ShiftProjectionCacheFactory : IShiftProjectionCacheFactory
	{
		public ShiftProjectionCache Create(IWorkShift workShift, IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = null)
		{
			return new ShiftProjectionCache(workShift, dateOnlyAsDateTimePeriod);
		}
	}


	public class ShiftProjectionCacheFactoryOld : IShiftProjectionCacheFactory
	{
		public ShiftProjectionCache Create(IWorkShift workShift, IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = null)
		{
			return new ShiftProjectionCacheOld_KeepProjectionState(workShift, dateOnlyAsDateTimePeriod);
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_XXL_76496)]
	public interface IShiftProjectionCacheFactory
	{
		ShiftProjectionCache Create(IWorkShift workShift, IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = null);
	}
}