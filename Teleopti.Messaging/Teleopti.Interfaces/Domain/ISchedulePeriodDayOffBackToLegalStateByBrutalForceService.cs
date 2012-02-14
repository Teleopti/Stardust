using System.Collections;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Gets the schedule period day offs back to legal state bz using different methods.
    /// </summary>
    public interface ISchedulePeriodDayOffBackToLegalStateByBrutalForceService
    {
        /// <summary>
        /// Gets the schedule period day offs back to legal state by using every possible cases.
        /// </summary>
        void Execute(IScheduleMatrixPro matrix);

        /// <summary>
        /// Gets the <see cref="Execute"/> result.
        /// </summary>
        /// <value>The result.</value>
        bool? Result { get; }

        /// <summary>
        /// Gets the result array.
        /// </summary>
        /// <value>The result array.</value>
        BitArray ResultArray { get; }
    }
}