using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Coders
{
    /// <summary>
    /// Event Heartbeat Encoder.
    /// </summary>
    public interface IEventHeartbeatEncoder
    {
        /// <summary>
        /// Encode an Event Heratbeat.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        byte[] Encode(IEventHeartbeat item);
    }
}