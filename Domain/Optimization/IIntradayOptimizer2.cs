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
		DateOnly? Execute();

        /// <summary>
        /// Gets the container owner.
        /// </summary>
        /// <value>The container owner.</value>
        IPerson ContainerOwner { get; }

	    void LockDay(DateOnly value);
    }
}