using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IShiftProjectionCacheManager
	{
		IList<IShiftProjectionCache> ShiftProjectionCachesFromRuleSets(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod,
																						IEnumerable<IWorkShiftRuleSet> ruleSets, bool forRestrictionsOnly, bool checkExcluded);

		IList<IShiftProjectionCache> ShiftProjectionCachesFromRuleSets(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod,
																										IRuleSetBag bag, bool forRestrictionsOnly, bool checkExcluded);

		IShiftProjectionCache ShiftProjectionCacheFromShift(IEditableShift shift, IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod);
	}
}
