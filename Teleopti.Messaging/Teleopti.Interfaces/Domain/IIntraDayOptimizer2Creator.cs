using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Creates the IIntradayOptimizer2 optimizers list.
    /// </summary>
    public interface IIntradayOptimizer2Creator
    {
        /// <summary>
        /// Creates the list of optimizers.
        /// </summary>
        /// <returns></returns>
        IList<IIntradayOptimizer2> Create();
    }
}