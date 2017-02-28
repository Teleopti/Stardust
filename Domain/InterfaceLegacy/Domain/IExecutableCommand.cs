namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a command that can be executed
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2010-03-11
    /// </remarks>
    public interface IExecutableCommand
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-03-11
        /// </remarks>
        void Execute();
    }
}
