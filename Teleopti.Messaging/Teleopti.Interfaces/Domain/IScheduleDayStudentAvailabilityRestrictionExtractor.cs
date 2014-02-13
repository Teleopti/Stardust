using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Extract days with student availability restrictions
    /// </summary>
    public interface IScheduleDayStudentAvailabilityRestrictionExtractor
    {
        /// <summary>
        /// All unavailable student availability restrictions
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> AllUnavailable(IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// All available student availability restrictions
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> AllAvailable(IList<IScheduleDay> scheduleDays);

        /// <summary>
		/// All fulfilled student availability restrictions
        /// </summary>
        /// <param name="restrictionChecker"></param>
        /// <param name="scheduleDay"></param>
        /// <returns></returns>
        IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay);
    }
}
