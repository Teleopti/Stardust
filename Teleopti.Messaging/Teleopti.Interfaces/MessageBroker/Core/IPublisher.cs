using System;
using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{

    /// <summary>
    /// The publisher interface
    /// </summary>
    public interface IPublisher : IDisposable
    {

        /// <summary>
        /// Gets or sets the protocol.
        /// </summary>
        /// <value>The protocol.</value>
        MessagingProtocol Protocol { get; }

        /// <summary>
        /// Gets or sets the broker.
        /// </summary>
        /// <value>The broker.</value>
        IBrokerService Broker { get; set; }

        /// <summary>
        /// Subscribe to unhandled exceptions on background threads.
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler;

        /// <summary>
        /// Start the publisher.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 07/03/2010
        /// </remarks>
        void StartPublishing();

        /// <summary>
        /// Stop publishing messages
        /// </summary>
        void StopPublishing();

        /// <summary>
        /// Send an event message.
        /// </summary>
        /// <param name="messageInfo">The message info.</param>
        void Send(IMessageInformation messageInfo);

        /// <summary>
        /// Send an event message List.
        /// </summary>
        /// <param name="messages">The messages.</param>
        void Send(IList<IMessageInformation> messages);

    }

    /// <summary>
    /// Multicast Publisher interface.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IMulticastPublisher : IPublisher
    {

        /// <summary>
        /// Sends the byte array.
        /// </summary>
        /// <param name="byteArray">The byte array.</param>
        void SendByteArray(byte[] byteArray);

        /// <summary>
        /// Send an event message.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        void Send(IEventMessage eventMessage);

        /// <summary>
        /// Send an event message.
        /// </summary>
        /// <param name="messages">The messages.</param>
        void Send(IList<IEventMessage> messages);

    }

}