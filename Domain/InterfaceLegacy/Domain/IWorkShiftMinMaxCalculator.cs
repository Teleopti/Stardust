using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Calculates min and max allowed lengths for a workshift
    /// </summary>
    public interface IWorkShiftMinMaxCalculator
    {
        /// <summary>
        /// Determines whether the contained schedulematrix is in legal state.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <returns>
        /// 	<c>true</c> if [is in legal state]; otherwise, <c>false</c>.
        /// </returns>
        bool IsPeriodInLegalState(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions);

        /// <summary>
        /// Periods the legal state status.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <returns></returns>
        int PeriodLegalStateStatus(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions);

        /// <summary>
        /// Determines whether [is week in legal state] [the specified week index].
        /// </summary>
        /// <param name="weekIndex">Index of the week.</param>
        /// <param name="matrix">The matrix.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// 	<c>true</c> if [is week in legal state] [the specified week index]; otherwise, <c>false</c>.
        /// </returns>
        bool IsWeekInLegalState(int weekIndex, IScheduleMatrixPro matrix, SchedulingOptions options);

        /// <summary>
        /// Determines whether [is week in legal state] [the specified date in week].
        /// </summary>
        /// <param name="dateInWeek">The date in week.</param>
        /// <param name="matrix">The matrix.</param>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <returns>
        /// 	<c>true</c> if [is week in legal state] [the specified date in week]; otherwise, <c>false</c>.
        /// </returns>
        bool IsWeekInLegalState(DateOnly dateInWeek, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions);

        /// <summary>
        /// Calculates allowed minimum and maximum contract time to be scheduled on the specific date.
        /// </summary>
        /// <param name="dayToSchedule">The day to schedule.</param>
        /// <param name="matrix">The matrix.</param>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        MinMax<TimeSpan>? MinMaxAllowedShiftContractTime(
            DateOnly dayToSchedule, 
            IScheduleMatrixPro matrix,
            SchedulingOptions schedulingOptions);

        /// <summary>
        /// Possibles the min max time for period.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <returns></returns>
        MinMax<TimeSpan> PossibleMinMaxTimeForPeriod(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions);

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
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		IDictionary<DateOnly, MinMax<TimeSpan>> PossibleMinMaxWorkShiftLengths(IScheduleMatrixPro matrix,  SchedulingOptions schedulingOptions, IDictionary<DateOnly, TimeSpan> maxWorkTimeDictionary);
    }
}