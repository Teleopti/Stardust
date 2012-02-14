#pragma warning disable 168

// ReSharper disable MemberCanBeMadeStatic
// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable UnusedParameter.Local

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;
using System.Threading;
using Teleopti.Core;
using Teleopti.Logging;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Exceptions;
using Teleopti.Messaging.Protocols;

namespace Teleopti.Messaging.Server
{
    public class Publisher : MarshalByRefObject, IPublisher
    {
        #region Fields
        
        private const string TeleoptiPublisherThread = " Teleopti Publisher Thread";
        private EventHandler<UnhandledExceptionEventArgs> _unhandledException;
        private CustomThreadPool _threadPool;
        private readonly int _numberOfThreads;
        private readonly MessagingProtocol _messagingProtocol;
        private IBrokerService _broker;
        private readonly int _serverThrottle;
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Publisher"/> class.
        /// </summary>
        /// <param name="serverThrottle">The server throttle.</param>
        /// <param name="messagingProtocol">The messaging protocol.</param>
        public Publisher(int serverThrottle, MessagingProtocol messagingProtocol) : this(1 /* use only one sender thread */, serverThrottle, messagingProtocol)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Publisher"/> class.
        /// </summary>
        /// <param name="numberOfThreads">The number of threads.</param>
        /// <param name="serverThrottle">The server throttle.</param>
        /// <param name="messagingProtocol">The messaging protocol.</param>
        protected Publisher(int numberOfThreads, int serverThrottle, MessagingProtocol messagingProtocol)
        {
            _numberOfThreads = numberOfThreads;
            _messagingProtocol = messagingProtocol;
            _serverThrottle = serverThrottle;
        }

        #endregion

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"/> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"/> property.
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">
        /// The immediate caller does not have infrastructure permission.
        /// </exception>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure"/>
        /// </PermissionSet>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 02/05/2010
        /// </remarks>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Gets or sets the broker.
        /// </summary>
        /// <value>The broker.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 07/03/2010
        /// </remarks>
        public IBrokerService Broker
        {
            get { return _broker; }
            set { _broker = value; }
        }

        /// <summary>
        /// Gets or sets the protocol.
        /// </summary>
        /// <value>The protocol.</value>
        public MessagingProtocol Protocol
        {
            get { return _messagingProtocol; }
        }

        /// <summary>
        /// Subscribe to unhandled exceptions on background threads.
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler
        {
            add { _unhandledException += value; }
            remove { _unhandledException -= value; }
        }

        #region Public Methods

        /// <summary>
        /// Start the publisher.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 07/03/2010
        /// </remarks>
        public void StartPublishing()
        {
            _threadPool = new CustomThreadPool(_numberOfThreads, TeleoptiPublisherThread);
            _threadPool.UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// Stop publishing messages
        /// </summary>
        public void StopPublishing()
        {
            if (_threadPool != null)
            {
                _threadPool.UnhandledException -= OnUnhandledException;
                _threadPool.Dispose();
                _threadPool = null;
            }
            if(_resetEvent != null)
            {
                _resetEvent.Close();
            }
            _broker = null;
        }

        /// <summary>
        /// Send an event message.
        /// </summary>
        /// <param name="messageInfo">The message info.</param>
        public void Send(IMessageInformation messageInfo)
        {
            QueueMessage(messageInfo);
        }

        /// <summary>
        /// Send an event message List.
        /// </summary>
        /// <param name="messages">The messages.</param>
        public void Send(IList<IMessageInformation> messages)
        {
            IDictionary<string, IList<IMessageInformation>> messagesUponAddress = SplitMessagesUponAddressAndPort(messages);
            QueueMessages(messagesUponAddress);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Queues the message.
        /// </summary>
        /// <param name="messageInfo">The message info.</param>
        private void QueueMessage(IMessageInformation messageInfo)
        {
            if (_threadPool != null)
            {
                _threadPool.QueueUserWorkItem(SendEventMessageInternal, messageInfo);
            }
            else
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Send(IMessageInfo messageInfo): Send thread pool is null. This can occur on application exit.");
            }
        }

        /// <summary>
        /// Queues the messages.
        /// </summary>
        /// <param name="messagesUponAddress">The messages upon address.</param>
        private void QueueMessages(IEnumerable<KeyValuePair<string, IList<IMessageInformation>>> messagesUponAddress)
        {
            foreach (KeyValuePair<string, IList<IMessageInformation>> keyValuePair in messagesUponAddress)
            {
                if (_threadPool != null)
                {
                    _threadPool.QueueUserWorkItem(SendEventMessagesInternal, keyValuePair.Value);
                }
                else
                {
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Send(IList<IMessageInfo> messages): TCP/IP thread pool is null. This occurs on application exit.");
                }
            }
        }

        /// <summary>
        /// Sends the event messages internal.
        /// </summary>
        /// <param name="state">The state.</param>
        private void SendEventMessagesInternal(object state)
        {
            IList<IMessageInformation> messages = (IList<IMessageInformation>)state;
            foreach (IMessageInformation message in messages)
            {
                if (message.EventMessage != null)
                {
                    IProtocol protocol = CreateProtocol(message);
                    protocol.SendPackage(message.EventMessage);
                    _resetEvent.WaitOne(_serverThrottle, false);
                }
            }
        }

        /// <summary>
        /// Sends the event message internal.
        /// </summary>
        /// <param name="state">The state.</param>
        private void SendEventMessageInternal(object state)
        {
            IMessageInformation message = (IMessageInformation)state;
            if (message.EventMessage != null)
            {
                IProtocol protocol = CreateProtocol(message);
                protocol.SendPackage(message.EventMessage);
                _resetEvent.WaitOne(_serverThrottle, false);
            }
        }

        /// <summary>
        /// Splits the messages upon address and port.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private IDictionary<string, IList<IMessageInformation>> SplitMessagesUponAddressAndPort(IEnumerable<IMessageInformation> messages)
        {
            IDictionary<string, IList<IMessageInformation>> dictionary = new Dictionary<string, IList<IMessageInformation>>();
            foreach (IMessageInformation message in messages)
            {
                string messagePort = message.Address + message.Port;
                IList<IMessageInformation> foundMessageInformation;
                if (dictionary.TryGetValue(messagePort,out foundMessageInformation))
                {
                    foundMessageInformation.Add(message);
                }
                else
                {
                    dictionary.Add(messagePort, new List<IMessageInformation>{message});
                }
            }
            return dictionary;
        }

        /// <summary>
        /// Gets the send protocol.
        /// </summary>
        /// <param name="messageInformation">The message information.</param>
        /// <returns></returns>
        private IProtocol CreateProtocol(IMessageInformation messageInformation)
        {
            switch (Protocol)
            {
                case MessagingProtocol.Udp:
                    return new UdpProtocol(Broker, messageInformation.Address, messageInformation.Port, messageInformation.TimeToLive);
                case MessagingProtocol.TcpIP:
                    return new TcpIpProtocol(Broker, messageInformation.Address, messageInformation.Port, messageInformation.TimeToLive);
                case MessagingProtocol.Multicast:
                    return new MulticastProtocol(Broker, messageInformation.Address, messageInformation.Port, messageInformation.TimeToLive);
                case MessagingProtocol.ClientTcpIP:
                    return new PollingProtocol(Broker, messageInformation.SubscriberId);
                default:
                    throw new MessageBrokerException("Protocol not supported.");
            }
        }

        /// <summary>
        /// Called when [unhandled exception].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_unhandledException != null)
                _unhandledException(this, e);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="isDisposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool isDisposed)
        {
            if (isDisposed)
                StopPublishing();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}









// ReSharper restore MemberCanBeMadeStatic
// ReSharper restore UnusedParameter.Local
// ReSharper restore MemberCanBeMadeStatic.Local
#pragma warning restore 168