using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
        /// <param name="matrix">The matrix.</param>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <returns></returns>
        MinMax<TimeSpan> PossibleLengthsForDate(DateOnly dateOnly, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions);

        ///<summary>
        ///</summary>
        void ResetCache();
    }
}