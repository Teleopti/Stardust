using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Calculates min and max allowed lengths for a workshift
    /// </summary>
    public interface IWorkShiftMinMaxCalculator
    {
        /// <summary>
        /// Determines whether the contained schedulematrix is in legal state.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is in legal state]; otherwise, <c>false</c>.
        /// </returns>
        bool IsPeriodInLegalState(IScheduleMatrixPro matrix);

        /// <summary>
        /// Gets the periods legal state status.
        /// </summary>
        /// <returns>
        /// /// <list type="bullet">
        /// 	<item>
        /// 		<description>-1: under legal state</description>
        /// 	</item>
        /// 	<item>
        /// 		<description>1: over legal state</description>
        /// 	</item>
        /// 	<item>
        /// 		<description>0: in legal state</description>
        /// 	</item>
        /// </list>
        /// </returns>
        int PeriodLegalStateStatus(IScheduleMatrixPro matrix);

        /// <summary>
        /// Determines whether [is week in legal state] [the specified week index].
        /// </summary>
        /// <param name="weekIndex">Index of the week.</param>
        /// <param name="matrix"></param>
        /// <returns>
        /// 	<c>true</c> if [is week in legal state] [the specified week index]; otherwise, <c>false</c>.
        /// </returns>
        bool IsWeekInLegalState(int weekIndex, IScheduleMatrixPro matrix);

        /// <summary>
        /// Determines whether [is week in legal state] [the specified date in week].
        /// </summary>
        /// <param name="dateInWeek">The date in week.</param>
        /// <param name="matrix"></param>
        /// <returns>
        /// 	<c>true</c> if [is week in legal state] [the specified date in week]; otherwise, <c>false</c>.
        /// </returns>
        bool IsWeekInLegalState(DateOnly dateInWeek, IScheduleMatrixPro matrix);

        /// <summary>
        /// Calculates allowed minimum and maximum contract time to be scheduled on the specific date.
        /// </summary>
        /// <param name="dayToSchedule">The day to schedule.</param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        MinMax<TimeSpan>? MinMaxAllowedShiftContractTime(DateOnly dayToSchedule, IScheduleMatrixPro matrix);

		/// <summary>
		/// Possibles the min max time for period.
		/// </summary>
		/// <returns></returns>
        MinMax<TimeSpan> PossibleMinMaxTimeForPeriod(IScheduleMatrixPro matrix);

        /// <summary>
        /// Gets the week count that is used in the IWorkShiftMinMaxCalculator.
        /// </summary>
        /// <value>The week count.</value>
        int WeekCount(IScheduleMatrixPro matrix);

        ///<summary>
        /// Empties the cashed values for all days
        ///</summary>
        void ResetCache();

		/// <summary>
		/// Possibles the min max work shift lengths.
		/// </summary>
		/// <param name="matrix">The matrix.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IDictionary<DateOnly, MinMax<TimeSpan>> PossibleMinMaxWorkShiftLengths(IScheduleMatrixPro matrix);
    }
}