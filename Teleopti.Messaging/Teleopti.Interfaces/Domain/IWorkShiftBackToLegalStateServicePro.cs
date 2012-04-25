using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Worksift back to legal state service
    /// </summary>
    public interface IWorkShiftBackToLegalStateServicePro
    {

        /// <summary>
        /// Executes the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <returns></returns>
        bool Execute(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions);

        /// <summary>
        /// Gets the removed days during the Execute.
        /// </summary>
        /// <value>The removed days.</value>
        IList<DateOnly> RemovedDays { get; }
    }
}