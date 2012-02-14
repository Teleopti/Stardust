using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Coders;
using Teleopti.Logging;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.Server
{
    

    [Serializable]
    public class MulticastPublisher : IMulticastPublisher
    {
        #region Fields

        private const string TeleoptiSocketThread = " Teleopti Socket Thread";

        [NonSerialized]
        private EventHandler<UnhandledExceptionEventArgs> _unhandledException;
        [NonSerialized]
        private CustomThreadPool _threadPool;
        private readonly int _timeToLive;
        private readonly int _numberOfThreads;
        [NonSerialized]
        private Socket _socket;
        private readonly string _address;
        private readonly int _port;

        #endregion

        #region Constructor

        public MulticastPublisher(string address, int port, int numberOfThreads, int timeToLive)
        {
            _address = SocketUtility.IsAddress(address) ? address : SocketUtility.GetIPAddressByHostName(address);
            _port = port;

            if (!SocketUtility.IsMulticastAddress(_address))
                throw new ArgumentException("Invalid multicast address.");

            _numberOfThreads = numberOfThreads;
            _timeToLive = timeToLive;
        }

        protected MulticastPublisher(SerializationInfo info, StreamingContext context)
        {
            _numberOfThreads = info.GetInt32("NumberOfThreads");
            _timeToLive = info.GetInt32("TimeToLive");
            _address = info.GetString("Address");
            _port = info.GetInt32("Port");
        }

        #endregion

        private void SendEventMessage(object state)
        {
            IEventMessage eventMessage = (IEventMessage)state;
            IEventMessageEncoder encoder = new EventMessageEncoder();
            SendByteArray(encoder.Encode(eventMessage));
        }

        private void SendEventMessages(object state)
        {
            IList<IEventMessage> eventMessages = (IList<IEventMessage>)state;
            foreach (IEventMessage eventMessage in eventMessages)
            {
                SendEventMessage(eventMessage);
            }
        }

        // ReSharper disable MemberCanBeMadeStatic
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void SendByteArray(object state)
        {
            byte[] bytes = (byte[])state;
            if (bytes != null)
            {
                try
                {
                    ISocketInfo socketInfo = new SocketInfo(_address, _port, 0);
                    socketInfo.IPAddress = IPAddress.Parse(_address);
                    socketInfo.TimeToLive = _timeToLive;
                    socketInfo.Socket = _socket;
                    if (socketInfo.IPAddress != null)
                        socketInfo.IPEndpoint = new IPEndPoint(socketInfo.IPAddress, socketInfo.Port);
                    if (socketInfo.Socket != null)
                        socketInfo.Socket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, socketInfo.IPEndpoint);
                }
                catch (ArgumentException exc)
                {
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                }
                catch (SocketException socketException)
                {
                    MessageBrokerException messageBrokerException = new MessageBrokerException(String.Format(CultureInfo.InvariantCulture, "WinSocket Error: {0}, Exception: {1}.", socketException.ErrorCode, socketException.Message), socketException);
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), messageBrokerException.Message);
                }
                catch (NullReferenceException nullReferenceException)
                {
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), nullReferenceException.ToString());
                }
                catch (Exception exc)
                {
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                }
            }
        }
        // ReSharper restore MemberCanBeMadeStatic

        public void SendByteArray(byte[] byteArray)
        {
            if (_threadPool != null)
            {
                _threadPool.QueueUserWorkItem(SendByteArray, byteArray);
            }
            else
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Send(byte[] values): Socket thread pool is null. This occurs on application exit.");
            }
        }

        public void Send(IEventMessage eventMessage)
        {
            SendEventMessage(eventMessage);
        }

        public void Send(IList<IEventMessage> messages)
        {
            SendEventMessages(messages);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_unhandledException != null)
                _unhandledException(this, e);
        }

        #region Public Methods

        public void StartPublishing()
        {
            _threadPool = new CustomThreadPool(_numberOfThreads, TeleoptiSocketThread);
            _threadPool.UnhandledException += OnUnhandledException;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, _timeToLive);
        }


        public event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler
        {
            add { _unhandledException += value; }
            remove { _unhandledException += value; }
        }

        public MessagingProtocol Protocol
        {
            get { return MessagingProtocol.Multicast; }
        }

        public void StopPublishing()
        {
            if (_threadPool != null)
                _threadPool.Dispose();
        }

        #endregion

        #region IDisposable Implementation

        protected virtual void Dispose(bool isDisposed)
        {
            if (isDisposed)
            {
                StopPublishing();
            }
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
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
            info.AddValue("NumberOfThreads", _numberOfThreads);
            info.AddValue("TimeToLive", _timeToLive);
            info.AddValue("Address", _address, _address.GetType());
            info.AddValue("Port", _port, _port.GetType());
        }

    }
}