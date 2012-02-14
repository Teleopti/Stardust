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
        /// <returns></returns>
        bool Resolve(IScheduleMatrixPro matrix); 
    }
}