using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Coders
{
    /// <summary>
    /// Encode an Event Subscription.
    /// </summary>
    public interface IEventSubscriberEncoder
    {
        /// <summary>
        /// Encodes an EventSubscriber object to a byte array.
        /// This method can throw an IOException upon invalid
        /// EventSubscriber property data.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        byte[] Encode(IEventSubscriber item);
    }
}