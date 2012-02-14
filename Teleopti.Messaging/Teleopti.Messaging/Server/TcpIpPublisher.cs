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
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.Server
{

    [Serializable]
    public class TcpIPPublisher : IUnicastPublisher
    {
        #region Fields
        private const string TeleoptiTcpIpThread = " Teleopti TCP/IP Thread";

        [NonSerialized]
        private EventHandler<UnhandledExceptionEventArgs> _unhandledException;
        [NonSerialized]
        private CustomThreadPool _threadPool;
        private readonly int _numberOfThreads;
        private IBrokerService _broker;

        #endregion

        #region Constructor

        public TcpIPPublisher(int numberOfThreads)
        {
            _numberOfThreads = numberOfThreads;
        }

        protected TcpIPPublisher(SerializationInfo info, StreamingContext context)
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
            get { return MessagingProtocol.TcpIP; }
        }

        #region Private Methods

        /// <summary>
        /// Sends the byte array.
        /// </summary>
        /// <param name="messageInfo">The message info.</param>
        private void SendMessageInfo(IMessageInfo messageInfo)
        {
            if (_threadPool != null)
            {
                _threadPool.QueueUserWorkItem(SendEventMessageInternal, messageInfo);
            }
            else
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Send(IMessageInfo messageInfo): TCP/IP thread pool is null. This can occur on application exit.");
            }
        }

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
                SendMessage(message);
            }
        }

        // don't make this static
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private IDictionary<string, IList<IMessageInfo>> SplitMessagesUponAddressAndPort(IEnumerable<IMessageInfo> messages)
        {
            IDictionary<string, IList<IMessageInfo>> dictionary = new Dictionary<string, IList<IMessageInfo>>();
            foreach (IMessageInfo message in messages)
            {
                string messagePort = message.Address + message.Port;
                if (dictionary.ContainsKey(messagePort))
                {
                    dictionary[messagePort].Add(message);
                }
                else
                {
                    dictionary.Add(messagePort, new List<IMessageInfo>());
                    dictionary[messagePort].Add(message);
                }
            }
            return dictionary;
        }


        // ReSharper disable MemberCanBeMadeStatic
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void SendMessage(IMessageInfo messageInfo)
        {
            if (messageInfo != null)
            {
                try
                {
                    IEventMessageEncoder encoder = new EventMessageEncoder();
                    messageInfo.Package = encoder.Encode(messageInfo.EventMessage);
                    using (TcpSender sender = new TcpSender())
                    {
                        try
                        {
                            IPAddress ipAddress = IPAddress.Parse(messageInfo.Address);
                            if (ipAddress != null)
                            {
                                sender.Connect(new IPEndPoint(ipAddress, messageInfo.Port));
                            }
                            else
                            {
                                throw new ArgumentException("IP Address string could not be parsed.");
                            }
                            NetworkStream networkStream = sender.GetStream();
                            networkStream.Write(messageInfo.Package, 0, messageInfo.Package.Length);
                            networkStream.Close();
                            sender.Close();
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
                catch (Exception exception)
                {
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exception.ToString());
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
        /// Send an event message.
        /// </summary>
        /// <param name="messageInfo">The message info.</param>
        public void Send(IMessageInfo messageInfo)
        {
            SendMessageInfo(messageInfo);
        }

        /// <summary>
        /// Send an event message List.
        /// </summary>
        /// <param name="messages">The messages.</param>
        public void Send(IList<IMessageInfo> messages)
        {
            IDictionary<string, IList<IMessageInfo>> messagesUponAddress = SplitMessagesUponAddressAndPort(messages);
            foreach (KeyValuePair<string, IList<IMessageInfo>> keyValuePair in messagesUponAddress)
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
        /// Start the publisher.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 07/03/2010
        /// </remarks>
        public void StartPublishing()
        {
            _threadPool = new CustomThreadPool(_numberOfThreads, TeleoptiTcpIpThread);
            _threadPool.UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// Stop publishing messages
        /// </summary>
        public void StopPublishing()
        {
            if (_threadPool != null)
                _threadPool.Dispose();
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