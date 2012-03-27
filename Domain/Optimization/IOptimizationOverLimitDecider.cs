using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Decides if the otimization is over the given limit
    /// </summary>
    public interface IOptimizationOverLimitDecider
    {
        /// <summary>
        /// Decides if the otimization is over the given limit
        /// </summary>
        /// <returns></returns>
        IList<DateOnly> OverLimit();
    }
}
