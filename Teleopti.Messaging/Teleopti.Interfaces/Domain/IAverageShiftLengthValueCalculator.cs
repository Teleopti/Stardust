using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Calculate a adjusted value depending on the shift length and agent average work time
    /// </summary>
    public interface IAverageShiftLengthValueCalculator
    {
        /// <summary>
        /// Calculates the shift value.
        /// </summary>
        /// <param name="shiftValue">The shift value.</param>
        /// <param name="workShiftWorkTime">The work shift work time.</param>
        /// <param name="averageWorkTime">The average work time.</param>
        /// <returns></returns>
        double CalculateShiftValue(double shiftValue, TimeSpan workShiftWorkTime, TimeSpan averageWorkTime);
    }
}