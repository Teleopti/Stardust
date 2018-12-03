using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
		/// <param name="rollbackService">The rollback service.</param>
		/// <returns></returns>
		bool Execute(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService);

        /// <summary>
        /// Gets the removed days during the Execute.
        /// </summary>
        /// <value>The removed days.</value>
        IList<DateOnly> RemovedDays { get; }

		/// <summary>
		/// Gets the removed schedules.
		/// </summary>
		IList<IScheduleDay> RemovedSchedules { get; }
    }
}