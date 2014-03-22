using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IShiftProjectionCacheManager
    {
       
        IList<IShiftProjectionCache> ShiftProjectionCachesFromRuleSetBag(DateOnly scheduleDateOnly, TimeZoneInfo timeZone,
                                                    IRuleSetBag bag, bool forRestrictionsOnly, bool checkExcluded);
    }
}
