using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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