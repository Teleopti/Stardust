using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
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

		//should be removed. if not possible for now, at least when toggle 37049 is removed (then IIntradayoptimizer2 could be removed completely)
		IntradayOptimizeOneday IntradayOptimizeOneday { get; }
    }
}