using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for classes to calculate the period target.
    /// </summary>
    public interface ISchedulePeriodTargetCalculator
    {
        /// <summary>
        /// Gets the target value of the schedule period identified by the dateOnly parameter.
        /// </summary>
        /// <param name="seasonality">if set to <c>true</c> [seasonality].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2009-10-19
        /// </remarks>
        TimeSpan PeriodTarget(bool seasonality);
    }
}
