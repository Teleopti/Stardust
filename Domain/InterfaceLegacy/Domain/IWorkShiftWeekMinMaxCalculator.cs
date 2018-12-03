using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Calculates min and max allowed shiftlength in a calendar week
    /// </summary>
    public interface IWorkShiftWeekMinMaxCalculator
    {
        /// <summary>
        /// Determines whether this week is in legal state.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is in legal state]; otherwise, <c>false</c>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        bool IsInLegalState(int weekIndex, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths, IScheduleMatrixPro matrix);

        /// <summary>
        /// Diff that needs to be taken care of om period base.
        /// </summary>
        /// <param name="weekIndex">Index of the week.</param>
        /// <param name="possibleMinMaxWorkShiftLengths">The possible min max work shift lengths.</param>
        /// <param name="matrix"></param>
        /// <param name="dayToSchedule">The day to schedule.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        TimeSpan CorrectionDiff(int weekIndex, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths, DateOnly? dayToSchedule, IScheduleMatrixPro matrix);

        /// <summary>
        /// Calculates the maximun allowed shift length to keep the week in legal state.
        /// Returns null if week is not in legal state
        /// </summary>
        /// <param name="weekIndex">Index of the week.</param>
        /// <param name="possibleMinMaxWorkShiftLengths">The possible min max work shift lengths.</param>
        /// <param name="dayToSchedule">The day to schedule.</param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        TimeSpan? MaxAllowedLength(int weekIndex, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths, DateOnly dayToSchedule, IScheduleMatrixPro matrix);
    }
}
