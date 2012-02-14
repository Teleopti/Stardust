using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.MessageBroker.Events
{
    /// <summary>
    /// Event Subscriber tells us who is subscribing and to what,
    /// one person can have multiple subscriptions.
    /// </summary>
    public interface IEventSubscriber : ISerializable
    {
        /// <summary>
        /// The unique subscriber Id. One user can have multiple Subscriptions.
        /// </summary>
        Guid SubscriberId { get; set; }

        /// <summary>
        /// The unique user id.
        /// </summary>
        Int32 UserId { get; set; }

        /// <summary>
        /// The process id of the subscriber.
        /// </summary>
        /// <value></value>
        Int32 ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        /// <value>The IP address.</value>
        string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        int Port { get; set; }

        /// <summary>
        /// Who changed/created the Subscription?
        /// </summary>
        string ChangedBy { get; set; }

        /// <summary>
        /// When was the subscription created/changed?
        /// </summary>
        DateTime ChangedDateTime { get; set; }

        /// <summary>
        /// Override of the ToString method
        /// </summary>
        /// <returns></returns>
        String ToString();
    }
}