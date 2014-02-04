using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Extract days with rotation restriction
    /// </summary>
    public interface IScheduleDayRotationRestrictionExtractor
    {
        /// <summary>
        /// All scheduleDays with a rotation restriction
        /// </summary>
        /// <returns></returns>
        IList<IScheduleDay> AllRestrictedDays(IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// All schedule days with a day off rotation restriction
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> AllRestrictedDayOffs(IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// All schedule days with a shift rotation restriction
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> AllRestrictedShifts(IList<IScheduleDay> scheduleDays);

        /// <summary>
		/// Schedule day with fulfilled restrictions
        /// </summary>
        /// <param name="restrictionChecker"></param>
        /// <param name="scheduleDay"></param>
        /// <returns></returns>
        IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay);

        /// <summary>
		/// Schedule day with fulfilled restriction day off
        /// </summary>
        /// <param name="restrictionChecker"></param>
        /// <param name="scheduleDay"></param>
        /// <returns></returns>
		IScheduleDay RestrictionFulfilledDayOff(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay);

        /// <summary>
		/// Schedule day with fulfilled restriction shift
        /// </summary>
        /// <param name="restrictionChecker"></param>
        /// <param name="scheduleDay"></param>
        /// <returns></returns>
		IScheduleDay RestrictionFulfilledShift(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay);
    }
}
