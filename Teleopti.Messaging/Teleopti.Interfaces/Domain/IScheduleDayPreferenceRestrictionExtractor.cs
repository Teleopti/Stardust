using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Extracts days with preference restrictions
    /// </summary>
    public interface IScheduleDayPreferenceRestrictionExtractor
    {
        /// <summary>
        /// All scheduleDays with a preference restriction
        /// </summary>
        /// <returns></returns>
        IList<IScheduleDay> AllRestrictedDays(IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// All schedule days with a absence preference restriction
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> AllRestrictedAbsences(IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// All schedule days with a day off preference restriction
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> AllRestrictedDayOffs(IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// All schedule days with a shift preference restriction
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> AllRestrictedShifts(IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// All schedule days with a must have preference
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> AllRestrictedDaysMustHave(IList<IScheduleDay> scheduleDays);

        /// <summary>
		/// Schedule day with fulfilled restrictions
        /// </summary>
        /// <param name="restrictionChecker"></param>
        /// <param name="scheduleDay"></param>
        /// <returns></returns>
        IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay);

       /// <summary>
		/// Schedule day with fulfilled restriction absence
       /// </summary>
       /// <param name="restrictionChecker"></param>
       /// <param name="scheduleDay"></param>
       /// <returns></returns>
		IScheduleDay RestrictionFulfilledAbsence(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay);

        /// <summary>
		/// Schedule day with fulfilled restriction day off
        /// </summary>
        /// <param name="restrictionChecker"></param>
        /// <param name="scheduleDay"></param>
        /// <returns></returns>
		IScheduleDay RestrictionFulfilledDayOff(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay);

        /// <summary>
		/// Schedule day with fulfilled restrction shift
        /// </summary>
        /// <param name="restrictionChecker"></param>
        /// <param name="scheduleDay"></param>
        /// <returns></returns>
		IScheduleDay RestrictionFulfilledShift(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay);

        /// <summary>
		/// Schedule day with fulfilled restriction must have
        /// </summary>
        /// <param name="restrictionChecker"></param>
        /// <param name="scheduleDay"></param>
        /// <returns></returns>
        IScheduleDay RestrictionFulfilledMustHave(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay);
    }
}
