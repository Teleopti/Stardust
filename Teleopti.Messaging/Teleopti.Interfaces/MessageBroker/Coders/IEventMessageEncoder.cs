using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Coders
{
    /// <summary>
    /// Encodes an Event Message.
    /// </summary>
    public interface IEventMessageEncoder
    {
        /// <summary>
        /// Encodes an Event Message into a byte array package.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        byte[] Encode(IEventMessage item);
    }
}