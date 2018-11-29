using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Holds a reference to a schedule matrix and records the matrix schedule state when constructed
    /// </summary>
    public interface IScheduleMatrixOriginalStateContainer
    {
        /// <summary>
        /// Gets the schedule matrix.
        /// </summary>
        /// <value>The schedule matrix.</value>
        IScheduleMatrixPro ScheduleMatrix { get; }

        /// <summary>
        /// Gets the old state of the period days.
        /// </summary>
        /// <value>The old state of the period days.</value>
        IDictionary<DateOnly, IScheduleDay> OldPeriodDaysState { get; }

        /// <summary>
        /// Determines whether the matrix is fully scheduled.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is fully scheduled]; otherwise, <c>false</c>.
        /// </returns>
        bool IsFullyScheduled();

        /// <summary>
        /// Gets or sets a value indicating whether this container is [still alive].
        /// </summary>
        /// <value><c>true</c> if [still alive]; otherwise, <c>false</c>.</value>
        bool StillAlive { get; set; }

        /// <summary>
        /// Counts the changed day off days in a schedule matrix.
        /// </summary>
        /// <returns></returns>
        double ChangedDayOffsPercent();

        /// <summary>
        /// Is the workshift changed.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        bool WorkShiftChanged(DateOnly dateOnly);

		/// <summary>
		/// The original work time before any changes.
		/// </summary>
		/// <returns></returns>
    	TimeSpan OriginalWorkTime();
    }
}