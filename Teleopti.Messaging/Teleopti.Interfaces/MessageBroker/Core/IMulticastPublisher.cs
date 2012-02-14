using System;
using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// Multicast Publisher interface.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IPublisher : IDisposable
    {
        /// <summary>
        /// Start the publisher using 20 threads.
        /// </summary>
        void StartPublishing();

        /// <summary>
        /// Stop publishing messages
        /// </summary>
        void StopPublishing();

        /// <summary>
        /// Send an event message.
        /// </summary>
        /// <param name="messageInfo">The message info.</param>
        void Send(IMessageInfo messageInfo);

        /// <summary>
        /// Send an event message.
        /// </summary>
        /// <param name="eventMessage"></param>
        void Send(IList<IMessageInfo> eventMessage);

        /// <summary>
        /// Subscribe to unhandled exceptions on background threads.
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler;

    }
}