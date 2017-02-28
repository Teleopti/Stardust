using System.Collections;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Validates a DayOff against a set of valid legal state rules.
    /// </summary>
    public interface IDayOffLegalStateValidator
    {
        ///// <summary>
        ///// Determines whether the specified date is valid.
        ///// </summary>
        ///// <param name="scheduleMatrix">The schedule matrix.</param>
        ///// <param name="dateOnly">The date only.</param>
        ///// <returns>
        ///// 	<c>true</c> if the specified date only is valid; otherwise, <c>false</c>.
        ///// </returns>
        //bool IsValid(IScheduleMatrixPro scheduleMatrix, DateOnly dateOnly);

        /// <summary>
        /// Determines whether the specified day in the specified array is valid.
        /// </summary>
        /// <param name="periodDays">The peiod days.</param>
        /// <param name="dayOffIndex">Index of the day off.</param>
        /// <returns>
        /// 	<c>true</c> if the specified periodDays is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     The day offs are set as <c>true</c> values in the array.
        /// </remarks>
        bool IsValid(BitArray periodDays, int dayOffIndex);

    }
}