using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Optimizes dayoffs.
    /// </summary>
    public interface IScheduleOptimizationService
    {
        /// <summary>
        /// Occurs when [report progress].
        /// </summary>
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        /// <summary>
        /// Executes this service.
        /// </summary>
        void Execute();
    }
}