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
        /// <param name="logWriter">The log writer.</param>
        /// <returns></returns>
        bool OverLimit();
    }
}
