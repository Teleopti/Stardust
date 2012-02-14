using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Teleopti.Messaging.Caching;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Interfaces.Coders;
using Teleopti.Messaging.Interfaces.Core;
using Teleopti.Messaging.Interfaces.Events;

namespace Teleopti.Messaging.Core
{
    public class UdpMulticastPublisher : IUdpMulticastPublisher
    {

        #region Fields

        private readonly IPAddress _destinationAddress;
        private readonly int _destinationPort;
        private readonly int _timeToLive;
        private Socket _sock;
        private IPEndPoint _ipEndPoint;
        private SocketQueueReader _backgroundScheduler;
        private CustomThreadPool _eventMessageThreadPool;
        private EventHandler<UnhandledExceptionEventArgs> _unhandledException;
        private EventHandler<BackgroundSchedulerEventArgs> _backgroundSchedulerExceptionHandler;
        #endregion

        #region Constructor

        public UdpMulticastPublisher(string address, int destinationPort, int timeToLive)
        {
            if (!SocketUtility.IsMulticastAddress(address))
                throw new ArgumentException("Invalid multicast address.", "address");
            _destinationAddress = IPAddress.Parse(address);
            _destinationPort = destinationPort;
            _timeToLive = timeToLive;
        }

        #endregion

        public event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler
        {
            add { _unhandledException += value; }
            remove { _unhandledException -= value; }
        }

        public event EventHandler<BackgroundSchedulerEventArgs> BackgroundSchedulerExceptionHandler
        {
            add { _backgroundSchedulerExceptionHandler += value; }
            remove { _backgroundSchedulerExceptionHandler -= value; }            
        }

        #region Private Methods

        private void SendInternal(object state)
        {
            byte[] bytes;
            IEventMessage eventMessage = state as IEventMessage;
            if (eventMessage != null)
            {
                bytes = CreateBytes(eventMessage);
                Send(bytes);
            }
            else
            {
                bytes = (byte[]) state;
                SendPackage(bytes);
            }
        }

        private void SendPackage(byte[] package)
        {
            if (_sock != null)
            {
                _sock.SendTo(package, 0, package.Length, SocketFlags.None, _ipEndPoint);
            }
            else
            {
                Logger.GetInstance().WriteLine(LoggingCategory.Warning, GetType(), String.Format(CultureInfo.CurrentCulture, "Publisher socket on {0} is null.", Thread.CurrentThread.Name ));
            }
        }

        // ReSharper disable MemberCanBeMadeStatic
        private byte[] CreateBytes(IEventMessage state)
        {
            IEventMessageEncoder encoder = new EventMessageEncoder();
            return encoder.Encode(state);
        }
        // ReSharper restore MemberCanBeMadeStatic

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_unhandledException != null)
                _unhandledException(this, e);
        }

        #endregion

        #region Public Methods

        public void Start(int threads)
        {
            _eventMessageThreadPool = new CustomThreadPool(threads, "Teleopti Serialization Thread");
            _eventMessageThreadPool.UnhandledException += new EventHandler<UnhandledExceptionEventArgs>(OnUnhandledException);

            _backgroundScheduler = new SocketQueueReader();
            _backgroundScheduler.ErrorEvent += new EventHandler<UnhandledExceptionEventArgs>(OnUnhandledException);
            _backgroundScheduler.Start("Teleopti Publisher Thread");
            // Multicast socket to sending
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Set the Time to Live                          
            _sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, _timeToLive);

            // Create an IP endpoint class instance
            _ipEndPoint = new IPEndPoint(_destinationAddress, _destinationPort);
        }

        public void Send(byte[] package)
        {
            _backgroundScheduler.Enqueue(new WaitCallback(SendInternal), package);
        }

        public void Send(IEventMessage message)
        {
            _eventMessageThreadPool.QueueUserWorkItem(new WaitCallback(SendInternal), message);
        }

        public void Stop()
        {
            if (_sock != null)
            {
                _backgroundScheduler.Dispose();
                _eventMessageThreadPool.Dispose();
                _sock.Close();
                _sock = null;
            }
        }

        #endregion

        #region IDisposable Implementation

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