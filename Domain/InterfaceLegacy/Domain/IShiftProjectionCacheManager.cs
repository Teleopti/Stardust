using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
	public interface IShiftProjectionCacheManager
	{
		IList<ShiftProjectionCache> ShiftProjectionCachesFromRuleSets(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod, IEnumerable<IWorkShiftRuleSet> ruleSets, bool checkExcluded);
		IList<ShiftProjectionCache> ShiftProjectionCachesFromRuleSets(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod, IRuleSetBag bag, bool forRestrictionsOnly, bool checkExcluded);
		ShiftProjectionCache ShiftProjectionCacheFromShift(IEditableShift shift, IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod);
	}
}
