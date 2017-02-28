namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for logging.
    /// </summary>
    public interface ILogWriter
    {
        /// <summary>
        /// Logs the message as info.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogInfo(string message);
    }
}