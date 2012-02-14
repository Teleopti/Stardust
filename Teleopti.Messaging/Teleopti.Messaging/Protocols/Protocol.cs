using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.Protocols
{
    /// <summary>
    /// The absract protocol.
    /// </summary>
    public abstract class Protocol : IProtocol
    {
        /// <summary>
        /// The broker service.
        /// </summary>
        private readonly IBrokerService _brokerService;
        private ManualResetEvent _resetEvent;
        private ISocketInfo _socketInformation;
        private int _clientThrottle;
        [NonSerialized]
        private EventHandler<UnhandledExceptionEventArgs> _unhandledException;
        [NonSerialized] 
        private EventHandler<EventMessageArgs> _eventMessageHandler;
        private ICustomThreadPool _messagingHandlerPool;
        private readonly string _address;
        private readonly int _timeToLive;
        private bool _isStarted;
        private int _port;
        private Guid _subscriberId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Protocol"/> class.
        /// </summary>
        protected Protocol()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Protocol"/> class.
        /// </summary>
        /// <param name="socketInformation">The socket information.</param>
        protected Protocol(ISocketInfo socketInformation)
        {
            _socketInformation = socketInformation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Protocol"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="socketInformation">The socket information.</param>
        protected Protocol(IBrokerService brokerService, Guid subscriberId, ISocketInfo socketInformation)
        {
            _brokerService = brokerService;
            _subscriberId = subscriberId;
            _socketInformation = socketInformation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Protocol"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        protected Protocol(IBrokerService brokerService, Guid subscriberId)
        {
            _brokerService = brokerService;
            _subscriberId = subscriberId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpIpProtocol"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="timeToLive">The time to live.</param>
        protected Protocol(IBrokerService brokerService, string address, int port, int timeToLive)
        {
            _brokerService = brokerService;
            _timeToLive = timeToLive;
            _port = port;
            _address = address;
        }

        /// <summary>
        /// Subscribe to unhandled exceptions on background threads.
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler
        {
            add { _unhandledException += value; }
            remove { _unhandledException -= value; }
        }

        /// <summary>
        /// Receive Event Messages
        /// </summary>
        public event EventHandler<EventMessageArgs> EventMessageHandler
        {
            add { _eventMessageHandler += value; }
            remove { _eventMessageHandler -= value; }
        }

        /// <summary>
        /// Signal close down of eternal message loop.
        /// </summary>
        /// <value></value>
        public ManualResetEvent StopReceiving
        {
            get { return _resetEvent; }
        }

        /// <summary>
        /// Gets the address.
        /// </summary>
        /// <value>The address.</value>
        public string Address
        {
            get { return _address; }
        }

        /// <summary>
        /// Gets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        /// <summary>
        /// Gets the time to live.
        /// </summary>
        /// <value>The time to live.</value>
        public int TimeToLive
        {
            get { return _timeToLive; }
        }

        /// <summary>
        /// Gets the broker service.
        /// </summary>
        /// <value>The broker service.</value>
        public IBrokerService BrokerService
        {
            get { return _brokerService; }
        }

        /// <summary>
        /// Gets the subscriber id.
        /// </summary>
        /// <value>The subscriber id.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public Guid SubscriberId
        {
            get { return _subscriberId; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is started.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is started; otherwise, <c>false</c>.
        /// </value>
        public bool IsStarted
        {
            get { return _isStarted; }
            set { _isStarted = value; }
        }

        /// <summary>
        /// Gets or sets the reset event.
        /// </summary>
        /// <value>The reset event.</value>
        public ManualResetEvent ResetEvent
        {
            get { return _resetEvent; }
            set { _resetEvent = value; }
        }

        /// <summary>
        /// Gets or sets the socket information.
        /// </summary>
        /// <value>The socket information.</value>
        public ISocketInfo SocketInformation
        {
            get { return _socketInformation; }
            set { _socketInformation = value; }
        }

        /// <summary>
        /// Gets or sets the client throttle.
        /// </summary>
        /// <value>The client throttle.</value>
        public int ClientThrottle
        {
            get { return _clientThrottle; }
            set { _clientThrottle = value; }
        }

        /// <summary>
        /// Gets the messaging handler pool.
        /// </summary>
        /// <value>The messaging handler pool.</value>
        public ICustomThreadPool MessagingHandlerPool
        {
            get { return _messagingHandlerPool; }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public abstract void SendPackage(IEventMessage eventMessage);

        /// <summary>
        /// Deregisters the subscriber.
        /// </summary>
        protected void DeregisterSubscriber()
        {
            _brokerService.UnregisterSubscriber(Address, Port);
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        public virtual void Initialise(ICustomThreadPool customThreadPool)
        {
            _messagingHandlerPool = customThreadPool;
            IsStarted = true;
            ResetEvent = new ManualResetEvent(false);
            SocketInformation.IPAddress = IPAddress.Parse(SocketUtility.IsIpAddress(SocketInformation.Address) ? SocketInformation.Address : SocketUtility.GetIPAddressByHostName(SocketInformation.Address));
            ClientThrottle = SocketInformation.ClientThrottle;
            StartListener();
        }

        /// <summary>
        /// Decode the byte stream to an EventMessage.
        /// </summary>
        /// <param name="state"></param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void DecodeEventMessage(object state)
        {
            byte[] package = (byte[])state;
            IEventMessage eventMessage = null;
            try
            {
                EventMessageDecoder decoder = new EventMessageDecoder();
                eventMessage = decoder.Decode(package);
            }
            catch (Exception)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), "Could not decode event message.");
            }
            if(eventMessage != null)
            {
                NotifyUser(eventMessage);
            }
        }

        /// <summary>
        /// Decode the byte stream to an EventMessage.
        /// </summary>
        /// <param name="state"></param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void HandleEventMessage(object state)
        {
            if (state != null)
            {
                IEventMessage[] eventMessages = (IEventMessage[])state;
                for (int i = 0; i < eventMessages.Length; i++)
                {
                    if (eventMessages[i] != null)
                        NotifyUser(eventMessages[i]);
                }
            }
        }


        /// <summary>
        /// Notify the User by invoking the delegate.
        /// </summary>
        /// <param name="eventMessage"></param>
        private void NotifyUser(IEventMessage eventMessage)
        {
            if (_eventMessageHandler != null)
                _eventMessageHandler(this, new EventMessageArgs(eventMessage));
        }

        // ReSharper disable MemberCanBeMadeStatic
        public void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_unhandledException != null)
                _unhandledException(this, e);
        }

        /// <summary>
        /// Starts the listener.
        /// </summary>
        protected virtual void StartListener()
        {
        }

        /// <summary>
        /// Reads the byte stream.
        /// </summary>
        public abstract void ReadByteStream();
        /// <summary>
        /// Stops the subscribing.
        /// </summary>
        public virtual void StopSubscribing()
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 29/04/2010
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 29/04/2010
        /// </remarks>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if(_messagingHandlerPool != null)
                {
                    _messagingHandlerPool.Dispose();
                    _messagingHandlerPool = null;
                }
                if (_resetEvent != null)
                {
                    _resetEvent.Close();
                    _resetEvent = null;
                }
                if (_socketInformation != null &&
                    _socketInformation.Socket != null)
                {
                    _socketInformation.Socket.Close();
                }
            }
        }

    }
}
