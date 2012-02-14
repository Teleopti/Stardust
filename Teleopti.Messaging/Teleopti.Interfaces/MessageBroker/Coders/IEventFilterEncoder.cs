using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Coders
{
    /// <summary>
    /// Event Filter Encoder.
    /// </summary>
    public interface IEventFilterEncoder
    {
        /// <summary>
        /// Encode message that takes the IEventFilter 
        /// and turns it into a byte array.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        byte[] Encode(IEventFilter item);
    }
}