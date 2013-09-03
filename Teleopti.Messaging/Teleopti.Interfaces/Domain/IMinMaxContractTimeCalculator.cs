using System;
using System.Diagnostics.CodeAnalysis;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for MinMaxContractTimeCalculator
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-05-13
    /// </remarks>
    public interface IMinMaxContractTimeCalculator
    {
        /// <summary>
        /// Return the MinMax contracttime of the date
        /// </summary>
        /// <param name="workShiftDate">The work shift date.</param>
        /// <param name="resultStateHolder"></param>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-05-13
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        MinMax<TimeSpan>? GetMinMaxContractTime(
            DateOnly workShiftDate,
            ISchedulingResultStateHolder resultStateHolder,
            ISchedulingOptions schedulingOptions);
    }
}