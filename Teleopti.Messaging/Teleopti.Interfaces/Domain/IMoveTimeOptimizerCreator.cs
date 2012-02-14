using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Creates the optimizers list.
    /// </summary>
    public interface IMoveTimeOptimizerCreator
    {
        /// <summary>
        /// Creates the list of optimizers.
        /// </summary>
        /// <returns></returns>
        IList<IMoveTimeOptimizer> Create();
    }
}