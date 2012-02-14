using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;          
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Messaging.Caching;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.Core
{
    [Serializable]
    public class MulticastPublisher : IMulticastPublisher, ISerializable
    {

        #region Fields

        private const string TeleoptiSocketThread = " Teleopti Socket Thread";

        [NonSerialized]
        private EventHandler<UnhandledExceptionEventArgs> _unhandledException;
        [NonSerialized]
        private SocketThreadPool _socketThreadPool;

        private readonly IList<ISocketInfo> _socketInfos;
        private readonly int _timeToLive;

        #endregion

        #region Constructor

        public MulticastPublisher(IList<ISocketInfo> socketInformation, int timeToLive)
        {
            _socketInfos = socketInformation;
            _timeToLive = timeToLive;   
        }

        protected MulticastPublisher(SerializationInfo info, StreamingContext context)
        {
            _socketInfos = (IList<ISocketInfo>)info.GetValue("SocketInfos", typeof(IList<ISocketInfo>));
            _timeToLive = info.GetInt32("TimeToLive");
        }

        #endregion

        public event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler
        {
            add { _unhandledException += value;  }
            remove { _unhandledException -= value;  }
        }

        #region Private Methods

        private void SendEventMessage(object state)
        {
            IEventMessage eventMessage = (IEventMessage) state;
            IEventMessageEncoder encoder = new EventMessageEncoder();
            byte[] bytes = encoder.Encode(eventMessage);
            Send(bytes);
        }

        private void SendEventMessages(object state)
        {
            IList<IEventMessage> eventMessages = (IList<IEventMessage>)state;
            IEventMessageEncoder encoder = new EventMessageEncoder();
            foreach (IEventMessage eventMessage in eventMessages)
            {
                byte[] bytes = encoder.Encode(eventMessage);
                Send(bytes);
            }
        }

        private void SendByteArray(object state)
        {
            byte[] package = (byte[])state;
            ISocketInfo socketInfo = _socketThreadPool.SocketInfo;
            if(socketInfo != null && socketInfo.Socket != null)
                socketInfo.Socket.SendTo(package, 0, package.Length, SocketFlags.None, socketInfo.IPEndpoint);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_unhandledException != null)
                _unhandledException(this, e);
        }

        #endregion

        #region Public Methods

        public void StartPublishing()
        {
            _socketThreadPool = new SocketThreadPool(TeleoptiSocketThread, _socketInfos);
            _socketThreadPool.UnhandledException += new EventHandler<UnhandledExceptionEventArgs>(OnUnhandledException);

            for (int i = 0; i < _socketInfos.Count; i++)
            {
                if (!SocketUtility.IsMulticastAddress(_socketInfos[i].Address))
                    throw new ArgumentException("Invalid multicast address.");

                _socketInfos[i].IPAddress = IPAddress.Parse(_socketInfos[i].Address);
                _socketInfos[i].TimeToLive = _timeToLive;
                _socketInfos[i].Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _socketInfos[i].Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1); 
                _socketInfos[i].Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, _timeToLive);
                _socketInfos[i].IPEndpoint = new IPEndPoint(_socketInfos[i].IPAddress, _socketInfos[i].Port);
            }
        }

        public void Send(byte[] values)
        {
            if(_socketThreadPool != null)
            {
                _socketThreadPool.QueueUserWorkItem(new WaitCallback(SendByteArray), values);
            }
            else
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Send(byte[] values): Socket thread pool is null. This occurs on application exit.");
            }
        }

        public void Send(IEventMessage eventMessage)
        {
            if (_socketThreadPool != null)
            {
                _socketThreadPool.QueueUserWorkItem(new WaitCallback(SendEventMessage), eventMessage);    
            }
            else
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Send(IEventMessage eventMessage): Socket thread pool is null!");
            }
        }

        public void Send(IList<IEventMessage> eventMessage)
        {
            if (_socketThreadPool != null)
            {
                _socketThreadPool.QueueUserWorkItem(new WaitCallback(SendEventMessages), eventMessage);
            }
            else
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Socket thread pool is null!");
            }
        }


        public void StopPublishing()
        {
            if (_socketInfos != null)
            {
                for (int i = 0; i < _socketInfos.Count; i++)
                {
                    _socketInfos[i].Socket.Close();
                    _socketInfos[i] = null;                    
                }
                if (_socketThreadPool != null)
                    _socketThreadPool.Dispose();

            }
        }

        #endregion

        #region IDisposable Implementation

        protected virtual void Dispose(bool isDisposed)
        {
            if (isDisposed)
                StopPublishing();
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
            info.AddValue("TimeToLive", _timeToLive);
        }
    }
}