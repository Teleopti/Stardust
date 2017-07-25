using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
        void LogInfo(Func<FormattableString> message);
    }
}