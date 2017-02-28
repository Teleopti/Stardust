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
        /// Gets the container owner.
        /// </summary>
        /// <value>The container owner.</value>
        IPerson ContainerOwner { get; }

		/// <summary>
		/// Returns the matrix we are working with
		/// </summary>
		IScheduleMatrixPro Matrix
		{
			get;
		}
    }
}