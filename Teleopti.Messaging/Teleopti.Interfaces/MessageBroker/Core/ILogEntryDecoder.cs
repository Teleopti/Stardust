using System.IO;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// Event log entry decoder.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IEventLogEntryDecoder
    {
        /// <summary>
        /// Decode method decodes a stream to an IEventLogEntry.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        ILogEntry Decode(Stream source);

        /// <summary>
        /// The overloaded Decode method takes a byte array transform it to a stream
        /// and sends it in to the overloaded method above and returns an IEventLogEntry.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        ILogEntry Decode(byte[] packet);
    }
}