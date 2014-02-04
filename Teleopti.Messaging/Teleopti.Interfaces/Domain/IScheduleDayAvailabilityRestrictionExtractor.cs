using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Extract days with availability restrictions
    /// </summary>
    public interface IScheduleDayAvailabilityRestrictionExtractor
    {
        /// <summary>
        /// All unavailable availability restrictions
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> AllUnavailable(IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// All available availability restrictions
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> AllAvailable(IList<IScheduleDay> scheduleDays);

        /// <summary>
		/// All fulfilledavailability restrictions
        /// </summary>
        /// <param name="restrictionChecker"></param>
        /// <param name="scheduleDay"></param>
        /// <returns></returns>
        IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay);
    }
}
