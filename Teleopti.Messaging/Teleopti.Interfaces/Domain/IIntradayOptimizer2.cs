using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Intraday optimization container, which contatins a logic to try to reschedule one day on one matrix
    /// </summary>
    public interface IIntradayOptimizer2
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        bool Execute();

        /// <summary>
        /// Gets the container owner.
        /// </summary>
        /// <value>The container owner.</value>
        IPerson ContainerOwner { get; }
    }
}