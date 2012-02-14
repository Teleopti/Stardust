using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Worksift back to legal state service
    /// </summary>
    public interface IWorkShiftBackToLegalStateServicePro
    {
        /// <summary>
        /// Executes the specified schedule matrix.
        /// </summary>
        /// <returns></returns>
        bool Execute(IScheduleMatrixPro matrix);

        /// <summary>
        /// Gets the removed days during the Execute.
        /// </summary>
        /// <value>The removed days.</value>
        IList<DateOnly> RemovedDays { get; }
    }
}