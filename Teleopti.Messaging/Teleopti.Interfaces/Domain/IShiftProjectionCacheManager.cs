using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IShiftProjectionCacheManager
    {
        /// <summary>
        /// Returns a list of IShiftProjectionCaches based on a RulesSetBag valid for a Day.
        /// </summary>
        /// <param name="scheduleDateOnly">The schedule date only.</param>
        /// <param name="timeZone">The timezone the person is in</param>
        /// <param name="bag">The bag the person has</param>
        /// <param name="forRestrictionsOnly">if true returns from RuleSetBags where OnlyForRestrictions = true, else where it is false</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> ShiftProjectionCachesFromRuleSetBag(DateOnly scheduleDateOnly, ICccTimeZoneInfo timeZone,
                                                    IRuleSetBag bag, bool forRestrictionsOnly);// IPerson person);
    }
}
