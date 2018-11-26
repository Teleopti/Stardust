using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Extracts the longest and shortest available workshift
    /// </summary>
    public interface IPossibleMinMaxWorkShiftLengthExtractor
    {
        // Possibles lengths for date.
        MinMax<TimeSpan> PossibleLengthsForDate(DateOnly dateOnly, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, IDictionary<DateOnly, TimeSpan> maxWorkTimeDictionary);
        void ResetCache();
    }
}