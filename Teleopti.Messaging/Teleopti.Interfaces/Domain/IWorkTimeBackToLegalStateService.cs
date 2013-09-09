using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Put worktime back to legal state
    /// </summary>
    public interface IWorkTimeBackToLegalStateService
    {
		/// <summary>
		/// Executes the specified schedule day.
		/// </summary>
		/// <param name="scheduleMatrix">The schedule matrix.</param>
		/// <returns></returns>
        IList<IScheduleDayTracker> Execute(IScheduleMatrixPro scheduleMatrix);

		/// <summary>
		/// Executes the specified schedule matrix.
		/// </summary>
		/// <param name="scheduleMatrix">The schedule matrix.</param>
		/// <param name="schedulePartModifyAndRollbackService">The schedule part modify and rollback service.</param>
		/// <returns></returns>
		IList<IScheduleDayTracker> Execute(IScheduleMatrixPro scheduleMatrix, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService);
    }
}