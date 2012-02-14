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
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Client
{
    [Serializable]
    public class UdpSubscriber : ISubscriber
    {
        #region Fields

        private const string TeleoptiDeserialisationThread = " Teleopti Deserialisation Thread";
        private const string TeleoptiSubscriberThread = "Teleopti Subscriber Thread";

        private readonly IList<ISocketInfo> _socketInfos;

        [NonSerialized]
        private ManualResetEvent _resetEvent;

        [NonSerialized]
        private EventHandler<EventMessageArgs> _eventMessageHandler;

        [NonSerialized]
        private EventHandler<UnhandledExceptionEventArgs> _unhandledException;

        [NonSerialized]
        private CustomThreadPool _customThreadPool;

        [NonSerialized]
        private CustomThreadPool _socketThreadPool;

        [NonSerialized]
        private bool _isStarted;

        private readonly int _servers;
        private int _clientThrottle;
        private Thread _messageLoopThread;
        private int _port;

        #endregion

        #region Constructor

        public UdpSubscriber(IList<ISocketInfo> socketInformation)
        {
            _socketInfos = socketInformation;
            _servers = _socketInfos.Count;
        }

        protected UdpSubscriber(SerializationInfo info, StreamingContext context)
        {
            _socketInfos = (IList<ISocketInfo>)info.GetValue("SocketInfos", typeof(IList<ISocketInfo>));
            _servers = info.GetInt32("Servers");
        }

        #endregion

        #region Intialise Method

        private void Initialise(int numberOfThreads)
        {
            _isStarted = true;
            _resetEvent = new ManualResetEvent(false);
            foreach (ISocketInfo socketInfo in _socketInfos)
            {
                _clientThrottle = socketInfo.ClientThrottle;
                _port = socketInfo.Port;
            }
            _socketThreadPool = new CustomThreadPool(numberOfThreads, TeleoptiSubscriberThread);
            _socketThreadPool.UnhandledException += OnUnhandledException;
        }

        public int Port
        {
            get { return _port; }
        }

        #endregion

        #region Private Receive Implementation

        /// <summary>
        /// Start receiving but starting the 
        /// eternal receiving loop.
        /// </summary>
        private void Receive()
        {
            try
            {
                while (!_resetEvent.WaitOne(_clientThrottle, false))
                {
                    if (_socketInfos != null)
                    {
                        foreach (ISocketInfo current in _socketInfos)
                        {
                            if (current != null)
                                ReadByteStream(current);
                        }
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "TcpIp Subscriber thread interrupted!");
            }
            catch (ThreadAbortException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "TcpIp Subscriber thread aborted!");
            }
            catch (ObjectDisposedException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "TcpIp Subscriber thread disposed!");
            }
        }

        /// <summary>
        /// Read a byte stream from the socket.
        /// </summary>
        /// <param name="state"></param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ReadByteStream(object state)
        {
            ISocketInfo current = state as ISocketInfo;
            if (current != null)
            {
                try
                {
                    using (Socket receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                    {

                        // Set the reuse address option
                        receiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                        // Create an IPEndPoint and bind to it
                        receiveSocket.Bind(new IPEndPoint(IPAddress.Any, _port));

                        do
                        {
                            byte[] receiveBuffer = new byte[Consts.MaxWireLength];
                            try
                            {
                                receiveSocket.Receive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None);
                                _customThreadPool.QueueUserWorkItem(DecodeEventMessage, receiveBuffer);
                            }
                            catch (SocketException socketException)
                            {
                                Logger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "ErrorCode: {0}, Exception Description: {1}.", socketException.ErrorCode, socketException));
                            }
                        }
                        while (receiveSocket.Available > 0);
                    }
                }
                catch (SocketException socketException)
                {
                    Logger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "ErrorCode: {0}, Exception Description: {1}.", socketException.ErrorCode, socketException));
                }
                catch (Exception exc)
                {
                    Logger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
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

        #region Public Methods to implement ISubscriber

        public void StartSubscribing(int threads)
        {
            if (!_isStarted)
            {
                // If you have more than one MessageBroker applications running
                Initialise(_servers);

                _customThreadPool = new CustomThreadPool(threads, TeleoptiDeserialisationThread);
                _customThreadPool.UnhandledException += OnUnhandledException;

                // Call receive to start to subscribe to messages
                _messageLoopThread = new Thread(Receive);
                _messageLoopThread.Name = "Message Loop Thread";
                _messageLoopThread.IsBackground = true;
                _messageLoopThread.Start();
            }
        }

        public void StopSubscribing()
        {
            if (_socketInfos != null)
            {
                _isStarted = false;

                if (_socketThreadPool != null)
                    _socketThreadPool.Dispose();

                if (_customThreadPool != null)
                    _customThreadPool.Dispose();

                if (_resetEvent != null)
                    _resetEvent.Close();

                try
                {
                    if (_messageLoopThread != null)
                        _messageLoopThread.Interrupt();
                }
                catch (ThreadInterruptedException)
                {
                }
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

        #region Serialisation Method

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SocketInfos", _socketInfos, _socketInfos.GetType());
            info.AddValue("Servers", _socketInfos, _servers.GetType());
        }

        #endregion

    }
}