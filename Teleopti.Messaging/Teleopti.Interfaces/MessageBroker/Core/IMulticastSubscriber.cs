using System;
using System.Runtime.Serialization;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The Subscriber, which is a part of the core Message Broker implementation.
    /// </summary>
    public interface ISubscriber : IDisposable, ISerializable
    {
        /// <summary>
        /// Receive Event Messages
        /// </summary>
        event EventHandler<EventMessageArgs> EventMessageHandler;

        /// <summary>
        /// Subscribe to unhandled exceptions on background threads.
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler;

        /// <summary>
        /// Starts the subscriber, serialisation threads.
        /// A low number of threads would be sufficient, e.g. 1 - 3 threads.
        /// </summary>
        /// <param name="threads">The number of threads you want handling incomming messages</param>
        void StartSubscribing(int threads);

        /// <summary>
        /// Stop subscribing to event messages.
        /// </summary>
        void StopSubscribing();

    }
}