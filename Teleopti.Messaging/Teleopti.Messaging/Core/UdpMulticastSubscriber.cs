using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Teleopti.Messaging.Caching;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Interfaces.Core;
using Teleopti.Messaging.Interfaces.Events;

namespace Teleopti.Messaging.Core
{
    public class UdpMulticastSubscriber : IUdpMulticastSubscriber
    {
        #region Fields

        private readonly static object _lockObject = new object();
        private readonly IPAddress _address;
        private readonly int _port;
        private EventHandler<EventMessageArgs> _eventMessageHandler;
        private EventHandler<UnhandledExceptionEventArgs> _unhandledException;
        private readonly EventHandler<UnhandledExceptionEventArgs> _internalException;
        private Socket _sock;
        private IPEndPoint _ipEndPoint;
        private CustomThreadPool _subscriptionThreadPool;
        private readonly Thread _multicastSubscriberThread;
        

        #endregion

        #region Constructor

        public UdpMulticastSubscriber(string address, int port)
        {
            if (!SocketUtility.IsMulticastAddress(address))
                throw new ArgumentException("Invalid multicast address.", "address");
            _address = IPAddress.Parse(address);
            _port = port;
            _multicastSubscriberThread = new Thread(new ThreadStart(Receive));
            _multicastSubscriberThread.IsBackground = true;
            _multicastSubscriberThread.Name = "Multicast Subscriber Thread";
            _internalException += new EventHandler<UnhandledExceptionEventArgs>(OnInternalException);
        }

        #endregion

        #region Private Receive Implementation

        private void Bind()
        {

            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); // Multicast receiving socket

            // Set the reuse address option
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            // Create an IPEndPoint and bind to it
            _ipEndPoint = new IPEndPoint(IPAddress.Any, _port);
            _sock.Bind(_ipEndPoint);

            // Add membership in the multicast group
            _sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(_address, IPAddress.Any));

        }

        
        /// <summary>
        /// Eternal receiving loop.
        /// </summary>
        // ReSharper disable FunctionNeverReturns
        private void Receive()
        {
            while (true) 
            {
                try
                {
                    ReadByteStream();
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }

            }
        }
        // ReSharper restore FunctionNeverReturns

        /// <summary>
        /// Read a byte stream from the socket.
        /// </summary>
        private void ReadByteStream()
        {
            try
            {
                IPEndPoint receivePoint = new IPEndPoint(IPAddress.Any, 0);
                EndPoint tempReceivePoint = receivePoint;
                // Create and receive a datagram
                byte[] packet = new byte[EventMessageConst.MaxWireLength];
                lock (_lockObject)
                {
                    if (_sock == null)
                        throw new SocketIsNullException("Subscriber socket is null.");
                    if (_sock.Available > 0)
                    {
                        _sock.ReceiveFrom(packet, 0, EventMessageConst.MaxWireLength, SocketFlags.None, ref tempReceivePoint);
                        // Decode byte stream
                        _subscriptionThreadPool.QueueUserWorkItem(new WaitCallback(DecodeEventMessage), packet);
                    }   
                }
            }
            catch(SocketIsNullException)
            {
                //Logger.GetInstance().WriteLine(LoggingCategory.Warning, GetType(), "Socket is null.");
            }
            catch(ThreadInterruptedException)
            {
                //Logger.GetInstance().WriteLine(LoggingCategory.Warning, GetType(), "Shutdown of multicast subscriber.");
                throw;
            }
            catch (Exception exc)
            {
                _internalException(this, new UnhandledExceptionEventArgs(exc, false));
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
        
        private void OnInternalException(object sender, UnhandledExceptionEventArgs e)
        {
            OnUnhandledException(sender, e);
        }

        // ReSharper restore MemberCanBeMadeStatic

        #endregion

        #region Public Properties

        public bool IsAlive
        {
            get
            {
                bool alive = false;
                try
                { 
                    alive = _multicastSubscriberThread.IsAlive;
                } 
                catch (Exception)
                {
                    
                }
                return alive;
            }
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

        public void Start(int threads)
        {
            // Bind to socket
            Bind();

            _subscriptionThreadPool = new CustomThreadPool(threads, "Multicast Subscriber Thread ");
            EventHandler<UnhandledExceptionEventArgs> handler = new EventHandler<UnhandledExceptionEventArgs>(OnUnhandledException);
            _subscriptionThreadPool.UnhandledException += handler;
            
            // Call receive to start to subscribe to messages
            _multicastSubscriberThread.Start();
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (_sock != null)
                {
                    _multicastSubscriberThread.Interrupt();
                    // Drop membership in the multicast group
                    if (_subscriptionThreadPool != null)
                        _subscriptionThreadPool.Dispose(); // This used to throw in tests previously
                    _sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(_address, IPAddress.Any));
                    _sock.Close();
                    _sock = null;
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_resetEvent")]
        protected virtual void Dispose(bool isDisposed)
        {
            if (isDisposed)
                Stop();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}