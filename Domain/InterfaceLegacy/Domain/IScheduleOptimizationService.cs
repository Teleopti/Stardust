namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Optimizes dayoffs.
    /// </summary>
    public interface IScheduleOptimizationService
    {
        /// <summary>
        /// Executes this service.
        /// </summary>
        void Execute();
    }
}