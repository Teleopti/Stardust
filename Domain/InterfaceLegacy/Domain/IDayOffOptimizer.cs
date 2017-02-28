namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Performs a complete day off move. Moves a set of days decided by the decisionmaker (mostly 1 or 2 days), then reschedule the days
    /// where those days offs had been moved from.
    /// </summary>
    public interface IDayOffOptimizer
    {
        /// <summary>
        /// Executes a optimizer.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="originalStateContainer">The original state container.</param>
        /// <returns>
        /// True if the move was successfull, false if not.
        /// </returns>
        bool Execute(IScheduleMatrixPro matrix, IScheduleMatrixOriginalStateContainer originalStateContainer);
    }
}