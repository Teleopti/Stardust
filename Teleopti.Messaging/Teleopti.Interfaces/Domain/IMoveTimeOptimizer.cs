namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Move time optimization container, which contatins a logic to try to do one move on one matrix
    /// </summary>
    public interface IMoveTimeOptimizer
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        bool Execute();

        /// <summary>
        /// Determines whether [is moved days under limit].
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