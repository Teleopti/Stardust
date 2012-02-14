using System.IO;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Coders
{
    /// <summary>
    /// Decodes an Event Subscriber.
    /// </summary>
    public interface IEventSubscriberDecoder
    {
        /// <summary>
        /// Decode method decodes a stream to an IEventSubscriber.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        IEventSubscriber Decode(Stream source);

        /// <summary>
        /// The overloaded Decode method takes a byte array transform it to a stream
        /// and sends it in to the overloaded method above and returns an IEventSubscriber.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        IEventSubscriber Decode(byte[] packet);
    }
}