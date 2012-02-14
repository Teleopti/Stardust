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
        /// Determines whether the moved days are over the limit.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is moved days under limit]; otherwise, <c>false</c>.
        /// </returns>
        bool MovedDaysOverMaxDaysLimit();

        /// <summary>
        /// Gets the container owner.
        /// </summary>
        /// <value>The container owner.</value>
        IPerson ContainerOwner { get; }
    }
}