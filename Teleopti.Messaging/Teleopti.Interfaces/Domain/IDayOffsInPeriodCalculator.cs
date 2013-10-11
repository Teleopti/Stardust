using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Interface for checking number off day offs scheduled
	/// </summary>
	public interface IDayOffsInPeriodCalculator
	{
		/// <summary>
		/// Counts the day offs on period.
		/// </summary>
		/// <param name="virtualSchedulePeriod">The virtual schedule period.</param>
		/// <returns></returns>
		//int CountDayOffsOnPeriod(IVirtualSchedulePeriod virtualSchedulePeriod);
		IList<IScheduleDay> CountDayOffsOnPeriod(IVirtualSchedulePeriod virtualSchedulePeriod);
		/// <summary>
		/// Determines whether [has correct number of days off] [the specified virtual schedule period].
		/// </summary>
		/// <param name="virtualSchedulePeriod">The virtual schedule period.</param>
		/// <param name="targetDaysOff">The target days off.</param>
		/// <param name="dayOffsNow">The day offs now.</param>
		/// <returns>
		/// 	<c>true</c> if [has correct number of days off] [the specified virtual schedule period]; otherwise, <c>false</c>.
		/// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
		bool HasCorrectNumberOfDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod, out int targetDaysOff, out IList<IScheduleDay> dayOffsNow);

		
        /// <summary>
        /// Outsides the or at minimum target daysoff.
        /// </summary>
        /// <param name="virtualSchedulePeriod">The virtual schedule period.</param>
        /// <returns></returns>
	    bool OutsideOrAtMinimumTargetDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod);


        /// <summary>
        /// Outsides the or at maximum target daysoff.
        /// </summary>
        /// <param name="virtualSchedulePeriod">The virtual schedule period.</param>
        /// <returns></returns>
	    bool OutsideOrAtMaximumTargetDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod);

		/// <summary>
		///  Get week periods sorted on day offs
		/// </summary>
		/// <param name="scheduleMatrixPro"></param>
		/// <returns></returns>
		IList<IDayOffOnPeriod> WeekPeriodsSortedOnDayOff(IScheduleMatrixPro scheduleMatrixPro);

		/// <summary>
		/// Counts days off on given period
		/// </summary>
		/// <param name="scheduleMatrixPro"></param>
		/// <param name="period"></param>
		/// <returns></returns>
		IDayOffOnPeriod CountDayOffsOnPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnlyPeriod period);
	}
}