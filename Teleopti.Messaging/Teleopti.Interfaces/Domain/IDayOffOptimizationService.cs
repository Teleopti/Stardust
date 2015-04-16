using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// DayOff Optimization Service
    /// </summary>
    public interface IDayOffOptimizationService
    {
        /// <summary>
        /// Occurs when [report progress].
        /// </summary>
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        /// <summary>
        /// Executes this service.
        /// </summary>
        /// <param name="optimizers">The optimizers.</param>
        void Execute(IEnumerable<IDayOffOptimizerContainer> optimizers);
    }
}