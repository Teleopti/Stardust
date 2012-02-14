using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Coders
{
    /// <summary>
    /// Event User Encoder.
    /// </summary>
    public interface IEventUserEncoder
    {
        /// <summary>
        /// Encode an Event Message.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        byte[] Encode(IEventUser item);
    }
}