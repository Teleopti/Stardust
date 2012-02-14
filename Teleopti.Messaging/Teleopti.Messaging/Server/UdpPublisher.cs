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
using Teleopti.Logging;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.Server
{

    [Serializable]
    public class UdpPublisher : IUnicastPublisher
    {
        #region Fields
        private const string TeleoptiUdpThread = " Teleopti UDP Thread";

        [NonSerialized]
        private EventHandler<UnhandledExceptionEventArgs> _unhandledException;

        [NonSerialized]
        private CustomThreadPool _threadPool;
        
        private readonly int _numberOfThreads;
        private IBrokerService _broker;

        #endregion

        #region Constructor

        public UdpPublisher(int numberOfThreads)
        {
            _numberOfThreads = numberOfThreads;
        }

        protected UdpPublisher(SerializationInfo info, StreamingContext context)
        {
            _numberOfThreads = info.GetInt32("NumberOfThreads");
        }

        #endregion

        public IBrokerService Broker
        {
            get { return _broker; }
            set { _broker = value; }
        }

        public event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler
        {
            add { _unhandledException += value; }
            remove { _unhandledException -= value; }
        }

        public MessagingProtocol Protocol
        {
            get { return MessagingProtocol.Udp; }
        }

        #region Private Methods

        private void SendEventMessageInternal(object state)
        {
            IMessageInfo messageInfo = (IMessageInfo)state;
            IEventMessageEncoder encoder = new EventMessageEncoder();
            messageInfo.Package = encoder.Encode(messageInfo.EventMessage);
            SendMessageInfo(messageInfo);
        }

        private void SendEventMessagesInternal(object state)
        {
            IList<IMessageInfo> messages = (IList<IMessageInfo>)state;
            foreach (IMessageInfo message in messages)
            {
                SendEventMessageInternal(message);
            }
        }

        // ReSharper disable MemberCanBeMadeStatic
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void SendMessageInfo(object state)
        {
            IMessageInfo messageInfo = (IMessageInfo)state;
            if (messageInfo != null)
            {
                try
                {
                    using (Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                    {
                        try
                        {
                            IPAddress ipAddress = IPAddress.Parse(messageInfo.Address);
                            if (ipAddress != null)
                            {
                                EndPoint sendEndPoint = new IPEndPoint(ipAddress, messageInfo.Port);
                                sendSocket.SendTo(messageInfo.Package, 0, messageInfo.Package.Length, SocketFlags.None, sendEndPoint);
                            }
                        }
                        catch (ArgumentException exc)
                        {
                            DeregisterSubscriber(messageInfo);
                            BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                        }
                        catch (SocketException socketException)
                        {
                            ClientNotConnectedException clientNotConnected = new ClientNotConnectedException(String.Format(CultureInfo.InvariantCulture, "Client {0} on port {1} is not connected. WinSocket Error: {2}, Inner Exception: {3}.", messageInfo.Address, messageInfo.Port, socketException.ErrorCode, socketException.Message), socketException);
                            DeregisterSubscriber(messageInfo);
                            BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), clientNotConnected.Message);
                        }
                        catch (NullReferenceException nullReferenceException)
                        {
                            DeregisterSubscriber(messageInfo);
                            BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), nullReferenceException.ToString());
                        }
                        catch (Exception exc)
                        {
                            BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                }
            }
        }

        private void DeregisterSubscriber(IMessageInfo addressInfo)
        {
            _broker.UnregisterSubscriber(addressInfo);
        }

        // ReSharper restore MemberCanBeMadeStatic

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_unhandledException != null)
                _unhandledException(this, e);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends the byte array.
        /// </summary>
        /// <param name="messageInfo">The message info.</param>
        public void SendMessageInfo(IMessageInfo messageInfo)
        {
            if (_threadPool != null)
            {
                _threadPool.QueueUserWorkItem(SendMessageInfo, messageInfo);
            }
            else
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Send(IMessageInfo messageInfo): TCP/IP thread pool is null. This can occur on application exit.");
            }
        }

        /// <summary>
        /// Send an event message.
        /// </summary>
        /// <param name="messageInfo">The message info.</param>
        public void Send(IMessageInfo messageInfo)
        {
            SendEventMessageInternal(messageInfo);
        }

        /// <summary>
        /// Send an event message.
        /// </summary>
        /// <param name="messages">The messages.</param>
        public void Send(IList<IMessageInfo> messages)
        {
            SendEventMessagesInternal(messages);
        }

        /// <summary>
        /// Start the publisher.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 07/03/2010
        /// </remarks>
        public void StartPublishing()
        {
            _threadPool = new CustomThreadPool(_numberOfThreads, TeleoptiUdpThread);
            _threadPool.UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// Stop publishing messages
        /// </summary>
        public void StopPublishing()
        {
            if (_threadPool != null)
            {
                _threadPool.Dispose();
            }
        }

        #endregion

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("NumberOfThreads", _numberOfThreads, _numberOfThreads.GetType());
        }

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