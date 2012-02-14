using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Logging.Core;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.DataAccessLayer;

namespace Teleopti.Messaging.Events
{
    public sealed class MessageBrokerImplementation : MessageBrokerBaseImplementation, IMessageBroker
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBrokerImplementation"/> class.
        /// </summary>
        private MessageBrokerImplementation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBrokerImplementation"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        private MessageBrokerImplementation(String connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBrokerImplementation"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        private MessageBrokerImplementation(IBrokerService brokerService) : this()
        {
            _brokerService = brokerService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBrokerImplementation"/> class.
        /// </summary>
        /// <param name="typeFilter">The type filter.</param>
        private MessageBrokerImplementation(IDictionary<Type, IList<Type>> typeFilter) : this()
        {
            _typeFilter = typeFilter;
            _isTypeFilterApplied = true;
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        /// <returns></returns>
        private Int32 Initialise()
        {
            // Create a channel for communicating with the remote object
            // Notice no port is specified on the client
            try
            {
                BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                serverProvider.TypeFilterLevel = TypeFilterLevel.Full;
                BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
                IDictionary properties = new Hashtable();
                properties.Add("port", 0);
                properties.Add("name", string.Format(CultureInfo.InvariantCulture, "MB Channel {0}", Guid.NewGuid()));
                TcpChannel channel = new TcpChannel(properties, clientProvider, serverProvider);
                ChannelServices.RegisterChannel(channel, false);
                    }
            catch (RemotingException exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), exc.Message + exc.StackTrace);
        }

            // Create an instance of the remote object
            _brokerService = (BrokerService)Activator.GetObject(typeof(BrokerService), String.Format(CultureInfo.CurrentCulture, "tcp://{0}:{1}/{2}", _server, _port, NameConstant));
            _messagingProtocol = _brokerService.Protocol;
            // Use the object
            if (_brokerService.Equals(null))
        {
                throw new BrokerNotInstantiatedException("Could not instantiate the Broker Service.");
        }

            InternalLog("Client successfully got Broker Service Handle.");

            _userId = _brokerService.RegisterUser(Environment.UserDomainName, (Environment.UserName.Length > 10 ? Environment.UserName.Substring(0, 10) : Environment.UserName));

            InternalLog(String.Format(CultureInfo.CurrentCulture, "Client successfully retrieved User Id of {0}.", _userId));

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = hostEntry.AddressList[0];
            _subscriberId = _brokerService.RegisterSubscriber(_userId, (Environment.UserName.Length > 10 ? Environment.UserName.Substring(0, 10) : Environment.UserName), Process.GetCurrentProcess().Id, ipAddress.ToString(), _port);

            InternalLog(String.Format(CultureInfo.CurrentCulture, "Client successfully  retrieved Subscriber Id of {0}.", _subscriberId));

            _subscriber = GetSubscriber(_brokerService.RetrieveSocketInformation());
            _subscriber.UnhandledExceptionHandler += OnUnhandledExceptionHandler;
            _subscriber.EventMessageHandler += OnEventMessage;
            Dispatcher = new MessageDispatcher(this, _brokerService);
            _subscriber.StartSubscribing(_threads);

            InternalLog(String.Format(CultureInfo.CurrentCulture, "Client successfully started the subscriber."));

            return 0;

        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        public static IMessageBroker GetInstance()
        {
            lock (_lockObject)
            {
                if (_messageBroker == null)
                {
                    _messageBroker = new MessageBrokerImplementation();
                }
            }
            return _messageBroker;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public static IMessageBroker GetInstance(string connectionString)
        {
            lock (_lockObject)
            {
                if (_messageBroker == null)
                {
                    _messageBroker = new MessageBrokerImplementation(connectionString);
                }
            }
            return _messageBroker;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="typeFilter">The type filter.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IMessageBroker GetInstance(IDictionary<Type, IList<Type>> typeFilter)
        {
            lock (_lockObject)
            {
                if (_messageBroker == null)
                {
                    _messageBroker = new MessageBrokerImplementation(typeFilter);
                }
            }
            return _messageBroker;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <returns></returns>
        public static IMessageBroker GetInstance(IBrokerService brokerService)
        {
            lock (_lockObject)
            {
                if (_messageBroker == null)
                {
                    _messageBroker = new MessageBrokerImplementation(brokerService);
                }
            }
            return _messageBroker;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        /// <param name="threads">The threads.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="protocol">The Messaging Protocol</param>
        /// <returns></returns>
        public static IMessageBroker GetInstance(string server, int port, int threads, string connectionString, MessagingProtocol protocol)
        {
            lock (_lockObject)
            {
                if (_messageBroker == null)
                {
                    _messageBroker = new MessageBrokerImplementation();
                    _messageBroker.StartMessageBroker(server, port, threads, connectionString);
                }
            }
            return _messageBroker;
        }

        /// <summary>
        /// Gets the subscriber.
        /// </summary>
        /// <param name="socketInformation">The socket information.</param>
        /// <returns></returns>
        // ReSharper disable MemberCanBeMadeStatic
        private ISubscriber GetSubscriber(IList<ISocketInfo> socketInformation)
        {
            switch (_messagingProtocol)
        {
                case MessagingProtocol.Multicast:            
                    return new MulticastSubscriber(socketInformation);
                case MessagingProtocol.TCPIP:
                    return new TcpIpSubscriber(socketInformation);
                default:
                    throw new BrokerNotInstantiatedException("Could not determine MessagingProtocol.");
        }
        }
        // ReSharper restore MemberCanBeMadeStatic

        /// <summary>
        /// throws BrokerNotInstantiatedException if it fails.
        /// Please catch this exception and propagate to user
        /// since it can be a valid exception.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void StartMessageBroker()
        {
            try
            {
                if (_brokerService == null)
                {
                    if (String.IsNullOrEmpty(_connectionString))
                        _connectionString = ConfigurationManager.AppSettings[ConnectionStringConstant];
                    if (!String.IsNullOrEmpty(_connectionString) && _initialised == -1)
                {
                        _customThreadPool.UnhandledException += OnUnhandledException;
                        InitialiseFromDatabase();
                        _initialised = Initialise();
                }
            }
        }
            catch (Exception exc)
                {
                throw new BrokerNotInstantiatedException("Broker could not be instantiated. ", exc);
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exceptionObject = (Exception) e.ExceptionObject;
            Log(new LogEntry(Guid.Empty, Process.GetCurrentProcess().Id, "Interprocess Message Failed.", exceptionObject.GetType().Name, exceptionObject.Message, exceptionObject.StackTrace, Environment.UserName, DateTime.Now));
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void StartMessageBroker(string server, int port, int threads, string connectionString)
        {
            try
            {
                _port = port;
                _server = server;
                _threads = threads;
                _connectionString = connectionString;
                _initialised = Initialise();
            }
            catch (Exception exc)
            {
                throw new BrokerNotInstantiatedException("Broker could not be instantiated. ", exc);
            }
        }

        private void InitialiseFromDatabase()
        {
            ConfigurationInfoReader reader = new ConfigurationInfoReader(_connectionString);
            IList<IConfigurationInfo> configurationInfos = reader.Execute();
            foreach (IConfigurationInfo configurationInfo in configurationInfos)
            {
                SetConfigurationInfo(configurationInfo);
            }
        }

        private void SetConfigurationInfo(IConfigurationInfo configurationInfo)
        {
            if(configurationInfo.ConfigurationType == NameConstant)
            {

                if (configurationInfo.ConfigurationName.ToLower(CultureInfo.CurrentCulture) == PortConstant.ToLower(CultureInfo.CurrentCulture))
                    _port = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.CurrentCulture);

                if (configurationInfo.ConfigurationName.ToLower(CultureInfo.CurrentCulture) == ServerConstant.ToLower(CultureInfo.CurrentCulture))
                    _server = configurationInfo.ConfigurationValue;

                if (configurationInfo.ConfigurationName.ToLower(CultureInfo.CurrentCulture) == ThreadsConstant.ToLower(CultureInfo.CurrentCulture))
                    _threads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.CurrentCulture);

                if (configurationInfo.ConfigurationName.ToLower(CultureInfo.CurrentCulture) == SubscriberTypeConstant.ToLower(CultureInfo.CurrentCulture))
                    _messagingProtocol = (MessagingProtocol) Enum.Parse(typeof(MessagingProtocol), Convert.ToString(configurationInfo.ConfigurationValue, CultureInfo.CurrentCulture), true);

            }
        }

        public bool Restart()
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                if (_initialised == -1)
        {
                    _initialised = Initialise();
        }
                return (_initialised == 0 ? true : false);
        }
            return false;
        }

        public bool IsInitialised
        {
            get { return (_initialised == 0 ? true : false); }
        }


        public Guid SubscriberId
        {
            get { return _subscriberId; }
            set { _subscriberId = value; }
        }

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public void Log(ILogEntry eventLogEntry)
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                _brokerService.Log(eventLogEntry.ProcessId,
                                   eventLogEntry.Description,
                                   eventLogEntry.Exception,
                                   eventLogEntry.Message,
                                   eventLogEntry.StackTrace,
                                   Environment.UserName);
            }
        }

        public string ServicePath
        {
            get { return _brokerService.ServicePath; }    
            }    

        private void UnregisterSubscriber(Guid subscriberId)
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                _brokerService.UnregisterSubscriber(subscriberId);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void StopMessageBroker()
        {
            try
            {
                if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
                {
                    if (_filters != null)
                    {
                        foreach (IList<IEventFilter> filters in _filters.Values)
                        {
                            foreach (IEventFilter filter in filters)
                            {
                                UnregisterFilter(filter.FilterId);        
                            }
                        }
                        _filters.Clear();
                    }
                    UnregisterSubscriber(_subscriberId);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                if (_subscriber != null)
                    _subscriber.Dispose();
            }

            try
            {
                _customThreadPool.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            _brokerService = null;

        }

        public IList<IConfigurationInfo> RetrieveConfigurations()
        {
            return CreateConfigurations();
        }

        public IList<IMessageInfo> RetrieveAddresses()
        {
            return CreateAddresses();
        }

        #region Private Method Implementations


        public void UpdateConfigurations(IList<IConfigurationInfo> configurations)
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                _brokerService.UpdateConfigurations(configurations);
            }
        }

        public void DeleteConfigurationItem(IConfigurationInfo configurationInfo)
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                _brokerService.DeleteConfiguration(configurationInfo);
            }            
        }

        public void UpdateAddresses(IList<IMessageInfo> addresses)
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                _brokerService.UpdateAddresses(addresses);
            }
        }

        public void DeleteAddressItem(IMessageInfo addressInfo)
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                _brokerService.DeleteAddresses(addressInfo);
            }    
        }

        public IEventHeartbeat[] RetrieveHeartbeats()
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                return _brokerService.RetrieveHeartbeats();
            }
            return new IEventHeartbeat[0];
        }

        public ILogbookEntry[] RetrieveLogbookEntries()
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                return _brokerService.RetrieveLogbookEntries();
            }
            return new ILogbookEntry[0];
        }

        public IEventUser[] RetrieveEventUsers()
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                return _brokerService.RetrieveEventUsers();
            }
            return new IEventUser[0];
        }

        public IEventReceipt[] RetrieveEventReceipt()
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                return _brokerService.RetrieveEventReceipt();
            }
            return new IEventReceipt[0];
        }

        public IEventSubscriber[] RetrieveSubscribers()
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                return _brokerService.RetrieveSubscribers();
            }
            return new IEventSubscriber[0];
        }

        public IEventFilter[] RetrieveFilters()
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialised == 0)
            {
                ServiceGuard(_brokerService);
                return _brokerService.RetrieveFilters();
            }
            return new IEventFilter[0];
        }

        private void ServiceGuard(IBrokerService service)
        {
            if (service == null)
            {
                _initialised = -1;
                throw new BrokerNotInstantiatedException("Broker Service is null.");
            }
        }

        private void InternalLog(string message)
        {
            ServiceGuard(_brokerService);
            _brokerService.Log(Process.GetCurrentProcess().Id, String.Format(CultureInfo.CurrentCulture, "{0},{1},{2}", DateTime.Now, EventLogEntryType.SuccessAudit, message), String.Empty, String.Empty, String.Empty, Environment.UserName);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose(bool isDisposing)
        {
            if(isDisposing) { StopMessageBroker(); }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            _messageBroker = null;
        }

        #endregion

    }
}
