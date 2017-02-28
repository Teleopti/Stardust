namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A Command that knows if it can execute
    /// </summary>
    public interface ICanExecute
    {
        /// <summary>
        /// Determines whether this instance can execute.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can execute; otherwise, <c>false</c>.
        /// </returns>
        bool CanExecute();
    }
}