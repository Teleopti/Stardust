namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Resolves night rest problems
    /// </summary>
    public interface INightRestWhiteSpotSolverService
    {
		/// <summary>
		/// Resolves the specified matrix.
		/// </summary>
		/// <param name="matrix">The matrix.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <param name="schedulePartModifyAndRollbackService">The schedule part modify and rollback service.</param>
		/// <returns></returns>
		bool Resolve(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService); 
    }
}