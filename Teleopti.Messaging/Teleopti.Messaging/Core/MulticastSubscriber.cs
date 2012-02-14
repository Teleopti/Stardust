using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Messaging.Caching;
using Teleopti.Messaging.Coders;

namespace Teleopti.Messaging.Core
{
    [Serializable]
    public class MulticastSubscriber : IMulticastSubscriber, ISerializable 
    {

        #region Fields
        private const string TeleoptiDeserialisationThread = " Teleopti Deserialisation Thread";
        private const string ArgumentExceptionConst = "Invalid multicast address. Index {0}";
        private const string TeleoptiSubscriberThread = "Teleopti Subscriber Thread";

        private readonly IList<ISocketInfo> _socketInfos;

        [NonSerialized]
        private ManualResetEvent _resetEvent;
        [NonSerialized]
        private EventHandler<UnhandledExceptionEventArgs> _handler;
        [NonSerialized]
        private EventHandler<EventMessageArgs> _eventMessageHandler;
        [NonSerialized]
        private EventHandler<UnhandledExceptionEventArgs> _unhandledException;
        [NonSerialized]
        private EventHandler<UnhandledExceptionEventArgs> _internalException;
        [NonSerialized]
        private CustomThreadPool _customThreadPool;
        [NonSerialized]
        private SocketThreadPool _socketThreadPool;
        [NonSerialized]
        private Thread _multicastSubscriberThread;
        [NonSerialized]
        private bool _isStarted;
        [NonSerialized]
        private static object _lockObject;

        #endregion

        #region Constructor

        public MulticastSubscriber(IList<ISocketInfo> socketInformation)
        {
            _socketInfos = socketInformation;
        }

        protected MulticastSubscriber(SerializationInfo info, StreamingContext context)
        {
            _socketInfos = (IList<ISocketInfo>) info.GetValue("SocketInfos", typeof(IList<ISocketInfo>));
        }

        #endregion

        #region Intialise Method

        private void Initialise()
        {
            _isStarted = true;
            _handler += new EventHandler<UnhandledExceptionEventArgs>(OnUnhandledException);
            _resetEvent = new ManualResetEvent(false);
            _lockObject = new object();
            _multicastSubscriberThread = new Thread(new ThreadStart(Receive));
            _multicastSubscriberThread.IsBackground = true;
            _multicastSubscriberThread.Name = TeleoptiSubscriberThread;
            _internalException += new EventHandler<UnhandledExceptionEventArgs>(OnInternalException);

            int index = 0;
            foreach (ISocketInfo socketInfo in _socketInfos)
            {
                if (!SocketUtility.IsMulticastAddress(socketInfo.Address))
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ArgumentExceptionConst, index));

                socketInfo.IPAddress = IPAddress.Parse(socketInfo.Address);
                socketInfo.Socket = Bind(socketInfo.IPAddress, socketInfo.Port);
                index++;
            }
            _socketThreadPool = new SocketThreadPool("Teleopti Socket Threads", _socketInfos);
        }

        #endregion

        #region Private Receive Implementation

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private Socket Bind(IPAddress address, int port)
        {

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); // Multicast receiving socket

            // Set the reuse address option
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            // Create an IPEndPoint and bind to it
            sock.Bind(new IPEndPoint(IPAddress.Any, port));

            // Add membership in the multicast group
            sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(address, IPAddress.Any));

            return sock;
        }

        /// <summary>
        /// Start receiving but starting the 
        /// eternal receiving loop.
        /// </summary>
        private void Receive()
        {
            try
            {
                while (!_resetEvent.WaitOne(50, false))
                {
                    // Utmärkt.
                    lock (_lockObject)
                    {
                        if (_socketInfos != null)
                            foreach (ISocketInfo current in _socketInfos)
                            {
                                if (current != null)
                                    _socketThreadPool.QueueUserWorkItem(new WaitCallback(ReadByteStream), current);
                            }
                    }
                    
                }
            }
            catch (ThreadInterruptedException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Multicast Subscriber thread interrupted!");
            }
            catch (ThreadAbortException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Multicast Subscriber thread aborted!");
            }
            catch (ObjectDisposedException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Multicast Subscriber thread disposed!");
            }
        }

        /// <summary>
        /// Read a byte stream from the socket.
        /// </summary>
        /// <param name="state"></param>
        private void ReadByteStream(object state)
        {
            ISocketInfo current = state as ISocketInfo;
            if (current != null)
            {
                IPEndPoint receivePoint = new IPEndPoint(IPAddress.Any, current.Port);
                EndPoint tempReceivePoint = receivePoint;
                // Create and receive a datagram
                byte[] packet = new byte[Consts.MaxWireLength];
                if (current.Socket != null && current.Socket.Available > 0)
                {
                    current.Socket.ReceiveFrom(packet, 0, Consts.MaxWireLength, SocketFlags.None, ref tempReceivePoint);
                    // Decode byte stream
                    _customThreadPool.QueueUserWorkItem(new WaitCallback(DecodeEventMessage), packet);
                }
            }
        }

        /// <summary>
        /// Decode the byte stream to an EventMessage.
        /// </summary>
        /// <param name="state"></param>
        private void DecodeEventMessage(object state)
        {
            byte[] package = (byte[])state;
            EventMessageDecoder decoder = new EventMessageDecoder();
            IEventMessage eventMessage = decoder.Decode(package);
            NotifyUser(eventMessage);
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
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_unhandledException != null)
                _unhandledException(this, e);
        }
        // ReSharper restore MemberCanBeMadeStatic

        private void OnInternalException(object sender, UnhandledExceptionEventArgs e)
        {
            OnUnhandledException(sender, e);
        }

        #endregion

        #region Public Properties

        public ManualResetEvent StopReceiving
        {
            get { return _resetEvent; }
        }

        public event EventHandler<EventMessageArgs> EventMessageHandler
        {
            add { _eventMessageHandler += value; }
            remove { _eventMessageHandler -= value; }
        }

        public event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler
        {
            add { _unhandledException += value; }
            remove { _unhandledException -= value; }            
        }

        #endregion

        #region Public Methods to implement IUdpMulticastSubscriber

        public void StartSubscribing(int threads)
        {
            if (!_isStarted)
            {
                Initialise();

                _customThreadPool = new CustomThreadPool(threads, TeleoptiDeserialisationThread);
                _customThreadPool.UnhandledException += _handler;

                // Call receive to start to subscribe to messages
                _multicastSubscriberThread.Start();
            }
        }

        public void StopSubscribing()
        {
            if (_socketInfos != null)
            {
                _isStarted = false;
                lock (_lockObject)
                {
                    for (int i = 0; i < _socketInfos.Count; i++)
                    {
                        // Drop membership in the multicast group
                        if (_socketInfos[i] != null && _socketInfos[i].Socket != null)
                        {
                            _socketInfos[i].Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(_socketInfos[i].IPAddress, IPAddress.Any));
                            _socketInfos[i].Socket.Close();
                            _socketInfos[i] = null;
                        }
                    }
                }


                if (_socketThreadPool != null)
                    _socketThreadPool.Dispose();

                if(_customThreadPool != null)
                    _customThreadPool.Dispose();

                if (_resetEvent != null)
                    _resetEvent.Close();
            }
        }

        #endregion

        #region IDisposable Implementation

        protected virtual void Dispose(bool isDisposed)
        {
            if (isDisposed)
                StopSubscribing();    
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SocketInfos", _socketInfos);
        }
    }
}