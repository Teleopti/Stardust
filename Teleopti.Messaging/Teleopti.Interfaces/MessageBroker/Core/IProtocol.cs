using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The abstract protocol interface.
    /// </summary>
    public interface IProtocol : IDisposable
    {

        #region Client Side

        /// <summary>
        /// Occurs when [unhandled exception handler].
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler;
        /// <summary>
        /// Occurs when [event message handler].
        /// </summary>
        event EventHandler<EventMessageArgs> EventMessageHandler;
        /// <summary>
        /// Gets or sets the reset event.
        /// </summary>
        /// <value>The reset event.</value>
        ManualResetEvent ResetEvent { get; set; }
        /// <summary>
        /// Gets or sets the client throttle.
        /// </summary>
        /// <value>The client throttle.</value>
        int ClientThrottle { get; set; }
        /// <summary>
        /// Initialises this instance.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Initialise")]
        void Initialise(ICustomThreadPool customThreadPool);
        /// <summary>
        /// Reads the byte stream.
        /// </summary>
        void ReadByteStream();
        /// <summary>
        /// Stops the subscribing.
        /// </summary>
        void StopSubscribing();
        /// <summary>
        /// Called when [unhandled exception].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        void OnUnhandledException(object sender, UnhandledExceptionEventArgs e);

        #endregion


        #region Server Side

        /// <summary>
        /// Gets the broker service.
        /// </summary>
        /// <value>The broker service.</value>
        IBrokerService BrokerService { get; }
        /// <summary>
        /// Gets the address.
        /// </summary>
        /// <value>The address.</value>
        string Address { get; }
        /// <summary>
        /// Gets the port.
        /// </summary>
        /// <value>The port.</value>
        int Port { get; }
        /// <summary>
        /// Gets the time to live.
        /// </summary>
        /// <value>The time to live.</value>
        int TimeToLive { get; }
        /// <summary>
        /// Sends the package.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void SendPackage(IEventMessage eventMessage);

        #endregion

    }
}