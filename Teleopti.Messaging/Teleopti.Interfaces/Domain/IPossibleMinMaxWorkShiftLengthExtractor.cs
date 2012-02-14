using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Extracts the longest and shortest available workshift
    /// </summary>
    public interface IPossibleMinMaxWorkShiftLengthExtractor
    {
        /// <summary>
        /// Possibles lentghs for date.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        MinMax<TimeSpan> PossibleLengthsForDate(DateOnly dateOnly, IScheduleMatrixPro matrix);

        ///<summary>
        ///</summary>
        void ResetCache();
    }
}