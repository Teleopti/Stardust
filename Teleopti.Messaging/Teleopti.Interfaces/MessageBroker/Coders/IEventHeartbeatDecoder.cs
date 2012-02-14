using System.IO;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Coders
{
    /// <summary>
    /// Heartbeat Decoder
    /// </summary>
    public interface IEventHeartbeatDecoder
    {
        /// <summary>
        /// Decode a stream of an Event Heartbeat.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        IEventHeartbeat Decode(Stream source);
        /// <summary>
        /// Decode a byte array package into an Event Hearbeat.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        IEventHeartbeat Decode(byte[] packet);
    }
}