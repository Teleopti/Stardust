using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Logging.Core;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Composites;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.DataAccessLayer;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Exceptions;
using Teleopti.Messaging.Server;
using Timer = System.Timers.Timer;

namespace Teleopti.Messaging.Client
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public abstract class MessageBrokerBase
    {
        // Private const strings
        private const string ConnectionStringConstant = "MessageBroker";
        private const string NameConstant = "TeleoptiBrokerService";
        private const string PortConstant = "Port";
        private const string ServerConstant = "Server";
        private const string ThreadsConstant = "Threads";
        private const string SubscriberTypeConstant = "SubscriberType";
        public const string RestartTimeConstant = "RestartTime";

        // Static fields

        // Private fields
        private Int32 _initialized = -1;
        private Int32 _remotingPort;
        private Int32 _messagingPort;
        private Int32 _threads;
        private Int32 _userId;
        private Guid _subscriberId;
        private string _server;
        private string _connectionString;
        private bool _isTypeFilterApplied;
        private MessagingProtocol _messagingProtocol;
        private CustomThreadPool _customThreadPool = new CustomThreadPool(1, "Message Broker Thread");
        private EventHandler<EventMessageArgs> _eventMessage;
        private EventHandler<UnhandledExceptionEventArgs> _exceptionHandler;
        private IBrokerService _brokerService;
        private ISubscriber _subscriber;
        private IMessageDispatcher _dispatcher;
        private IMessageRegistrationManager _messageRegistrationManager;
        private IMessageFilterManager _filterManager = new MessageFilterManager();
        private IStaticDataManager _staticDataManager;
        private readonly static object _heartbeatTimerLock = new object();
        private DateTime _lastHeartbeat;
        private Thread _heartbeatThread;
        private Timer _timer;
        private long _restartTime = 2;
        private readonly string _userName = Environment.UserName;

        protected internal static IMessageBroker MessageBroker { get; set; }

        /// <summary>
        /// throws BrokerNotInstantiatedException if it fails.
        /// Please catch this exception and propagate to user
        /// since it can be a valid exception.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void StartMessageBroker()
        {
            try
            {
                if (BrokerService == null)
                {
                    if (String.IsNullOrEmpty(ConnectionString))
                        ConnectionString = ConfigurationManager.AppSettings[ConnectionStringConstant];
                    if (!String.IsNullOrEmpty(ConnectionString) && Initialized == -1)
                    {
                        CustomThreadPool.UnhandledException += OnUnhandledException;
                        InitialiseFromDatabase();
                        Initialized = Initialize();
                        PostInitialize();
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(ConnectionString))
                            throw new DataException("Connection String to database not properly initialised.");
                    }
                }
            }
            catch (Exception exc)
            {
                throw new BrokerNotInstantiatedException("Broker could not be instantiated. ", exc);
            }
        }

        /// <summary>
        /// Stops the message broker.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void StopMessageBroker()
        {
            try
            {
                StopHeartbeatLoop();
                if (!String.IsNullOrEmpty(ConnectionString) && Initialized == 0)
                {
                    MessageRegistrationManager.UnregisterFilters();
                    UnregisterSubscriber(SubscriberId);
                }
                if (BrokerService != null)
                    BrokerService.Log(Process.GetCurrentProcess().Id, String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}{3}.", DateTime.Now, EventLogEntryType.Information, "Broker has been disconnected. Machine ", Environment.MachineName), string.Empty, string.Empty, string.Empty, _userName);

                UnregisterClient();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            if (!String.IsNullOrEmpty(ConnectionString) && Initialized == 0)
            {
                if (Subscriber != null)
                    Subscriber.Dispose();
            }
            try
            {
                CustomThreadPool.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            BrokerService = null;
            Initialized = -1;
        }

        protected virtual void UnregisterClient()
        {
        }

        /// <summary>
        /// Gets or sets the event message.
        /// </summary>
        /// <value>The event message.</value>
        public EventHandler<EventMessageArgs> EventMessage
        {
            get { return _eventMessage; }
            set { _eventMessage = value; }
        }

        /// <summary>
        /// Occurs when [event message handler].
        /// </summary>
        public event EventHandler<EventMessageArgs> EventMessageHandler
        {
            add { _eventMessage += value; }
            remove { _eventMessage -= value; }
        }

        /// <summary>
        /// Occurs when [exception handler].
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> ExceptionHandler
        {
            add { _exceptionHandler += value; }
            remove { _exceptionHandler -= value; }
        }

        /// <summary>
        /// Gets or sets the broker service.
        /// </summary>
        /// <value>The broker service.</value>
        public IBrokerService BrokerService
        {
            get { return _brokerService; }
            set { _brokerService = value; }
        }

        /// <summary>
        /// Gets or sets the dispatcher.
        /// </summary>
        /// <value>The dispatcher.</value>
        public IMessageDispatcher Dispatcher
        {
            get { return _dispatcher; }
            set { _dispatcher = value; }
        }

        /// <summary>
        /// Gets or sets the message registration.
        /// </summary>
        /// <value>The message registration.</value>
        public IMessageRegistrationManager MessageRegistrationManager
        {
            get { return _messageRegistrationManager; }
            set { _messageRegistrationManager = value; }
        }

        /// <summary>
        /// Gets or sets the filter manager.
        /// </summary>
        /// <value>The filter manager.</value>
        public IMessageFilterManager FilterManager
        {
            get { return _filterManager; }
            set { _filterManager = value; }
        }

        /// <summary>
        /// Gets or sets the static data.
        /// </summary>
        /// <value>The static data.</value>
        public IStaticDataManager StaticData
        {
            get { return _staticDataManager; }
            set { _staticDataManager = value; }
        }

        /// <summary>
        /// Gets or sets the subscriber.
        /// </summary>
        /// <value>The subscriber.</value>
        public ISubscriber Subscriber
        {
            get { return _subscriber; }
            set { _subscriber = value; }
        }

        public int Initialized
        {
            get { return _initialized; }
            set { _initialized = value; }
        }

        public int RemotingPort
        {
            get { return _remotingPort; }
            set { _remotingPort = value; }
        }

        public int Threads
        {
            get { return _threads; }
            set { _threads = value; }
        }

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public MessagingProtocol MessagingProtocol
        {
            get { return _messagingProtocol; }
            set { _messagingProtocol = value; }
        }

        public int MessagingPort
        {
            get { return _messagingPort; }
            set { _messagingPort = value; }
        }

        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public Guid SubscriberId
        {
            get { return _subscriberId; }
            set { _subscriberId = value; }
        }

        public CustomThreadPool CustomThreadPool
        {
            get { return _customThreadPool; }
            set { _customThreadPool = value; }
        }

        public bool IsTypeFilterApplied
        {
            get { return _isTypeFilterApplied; }
            set { _isTypeFilterApplied = value; }
        }

        public bool IsInitialized
        {
            get { return (Initialized == 0 ? true : false); }
        }

        public string ServicePath
        {
            get { return BrokerService.ServicePath; }
        }

        protected long RestartTime
        {
            get { return _restartTime; }
        }

        protected internal string UserName
        {
            get { return _userName; }
        }

        protected abstract int Initialize();
        protected abstract int InitializeUponRestart();

        /// <summary>
        /// Starts the heartbeat.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 30/06/2009
        /// </remarks>
        protected void StartHeartbeat()
        {
            _lastHeartbeat = DateTime.Now;
            CleanupIfRestarting();
            IEventHeartbeat heartbeat = new EventHeartbeat();
            ThreadPool.QueueUserWorkItem(SendThreadedHeartbeat, heartbeat);
            _heartbeatThread = new Thread(StartHeartbeatLoop);
            _heartbeatThread.IsBackground = true;
            _heartbeatThread.Name = ThreadPoolThreadSetting.HeartbeatThreadPoolThreads.ToString();
            _heartbeatThread.Start();
        }

        private void CleanupIfRestarting()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
            if (_heartbeatThread != null)
            {
                _heartbeatThread.Abort();
            }
        }

        // ReSharper disable MemberCanBeMadeStatic
        private void CheckConnection(object state, ElapsedEventArgs eventArgs)
        {
            lock (_heartbeatTimerLock)
            {
                if (DateTime.Now.Subtract(_lastHeartbeat).TotalMilliseconds > _restartTime)
                {
                    try
                    {
                        if (BrokerService != null)
                        {
                            BrokerService.Log(Process.GetCurrentProcess().Id,
                                              String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}{3}.", DateTime.Now,
                                                            EventLogEntryType.Error,
                                                            "Broker has been disconnected. Attempting reconnection of ",
                                                            Environment.MachineName),
                                              string.Empty,
                                              string.Empty,
                                              string.Empty,
                                              _userName);
                        }

                        IDataMapper processor = new DataMapper(_connectionString, RestartTime);
                        // double check, we really do not want to start anything unneccessary.
                        bool isSubscribing = processor.CheckSubscriptionStatus(SubscriberId);
                        
                        if (!isSubscribing)
                        {
                            Initialized = -1;
                            Restart();
                        }
                        
                        if (IsInitialized && BrokerService != null)
                            BrokerService.Log(Process.GetCurrentProcess().Id,
                                              String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}{3} successful.", DateTime.Now, EventLogEntryType.SuccessAudit, "Broker has been connected. Reconnection of ", Environment.MachineName),
                                              string.Empty,
                                              string.Empty,
                                              string.Empty,
                                              _userName);

                    }
                    catch (SocketException exception)
                    {
                        Initialized = -1;
                        BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), string.Format(CultureInfo.InvariantCulture, "An error occured while trying check connection with broker: {0}", exception.Message));
                    }
                }
            }
        }

        /// <summary>
        /// If you need to restart the message broker.
        /// </summary>
        /// <returns></returns>
        public bool Restart()
        {
            if (!String.IsNullOrEmpty(ConnectionString) && Initialized == -1)
            {
                Initialized = InitializeUponRestart();
                PostInitialize();
                MessageRegistrationManager.ReinitializeFilters();
                return (Initialized == 0 ? true : false);
            }
            return false;
        }

        protected abstract void PostInitialize();

        // ReSharper restore MemberCanBeMadeStatic

        private void StartHeartbeatLoop()
        {
            lock (_heartbeatTimerLock)
            {
                if (_timer == null)
                {
                    _timer = new Timer();
                    _timer.Interval = _restartTime;
                    _timer.AutoReset = true;
                    _timer.Elapsed += CheckConnection;
                }
                _timer.Enabled = true;
                _timer.Start();
            }
        }

        private void StopHeartbeatLoop()
        {
            lock (_heartbeatTimerLock)
            {
                if (_timer == null) return;
                _timer.Enabled = false;
                _timer.Stop();
            }
        }

        /// <summary>
        /// Retreives the broker service handle.
        /// </summary>
        protected void RetrieveBrokerServiceHandle()
        {
            // Create an instance of the remote object
            try
            {
                BrokerService = (BrokerService)Activator.GetObject(typeof(BrokerService), String.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}/{2}", Server, RemotingPort, NameConstant));
                MessagingProtocol = BrokerService.Protocol;
            }
            catch (Exception exc)
            {
                throw new BrokerNotInstantiatedException("The Message Broker Service is most likely not running.", exc);
            }
            InternalLog("Client successfully got Broker Service Handle.");
        }


        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <param name="eventLogEntry"></param>
        public void Log(ILogEntry eventLogEntry)
        {
            if (!String.IsNullOrEmpty(ConnectionString) && Initialized == 0)
            {
                BrokerService.Log(eventLogEntry.ProcessId,
                                   eventLogEntry.Description,
                                   eventLogEntry.Exception,
                                   eventLogEntry.Message,
                                   eventLogEntry.StackTrace,
                                   _userName);
            }
        }

        /// <summary>
        /// Unregisters the subscriber.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void UnregisterSubscriber(Guid subscriberId)
        {
            Subscriber.StopSubscribing();
            try
            {
                BrokerService.UnregisterSubscriber(subscriberId);
            }
            catch (Exception)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Information, null, "BrokerService proxy is null. This is normal when client is disconnecting.");
            }
        }

        /// <summary>
        /// Initialises from database.
        /// </summary>
        private void InitialiseFromDatabase()
        {
            ConfigurationInfoReader reader = new ConfigurationInfoReader(ConnectionString);
            IList<IConfigurationInfo> configurationInfos = reader.Execute();
            foreach (IConfigurationInfo configurationInfo in configurationInfos)
            {
                SetConfigurationInfo(configurationInfo);
            }

        }

        /// <summary>
        /// Sets the configuration info.
        /// </summary>
        /// <param name="configurationInfo">The configuration info.</param>
        private void SetConfigurationInfo(IConfigurationInfo configurationInfo)
        {
            if (configurationInfo.ConfigurationType == NameConstant)
            {

                if (configurationInfo.ConfigurationName.ToUpperInvariant() == PortConstant.ToUpperInvariant())
                    RemotingPort = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);

                if (configurationInfo.ConfigurationName.ToUpperInvariant() == ServerConstant.ToUpperInvariant())
                    _server = SocketUtility.IsIpAddress(configurationInfo.ConfigurationValue) ? configurationInfo.ConfigurationValue : SocketUtility.GetIPAddressByHostName(configurationInfo.ConfigurationValue);

                if (configurationInfo.ConfigurationName.ToUpperInvariant() == ThreadsConstant.ToUpperInvariant())
                    Threads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);

                if (configurationInfo.ConfigurationName.ToUpperInvariant() == SubscriberTypeConstant.ToUpperInvariant())
                    MessagingProtocol = (MessagingProtocol)Enum.Parse(typeof(MessagingProtocol), Convert.ToString(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture), true);

                if (configurationInfo.ConfigurationName.ToUpperInvariant() == RestartTimeConstant.ToUpperInvariant())
                    _restartTime = Convert.ToInt64(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);

            }
        }

        /// <summary>
        /// Sends the receipt.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendReceipt(IEventMessage message)
        {
            ServiceGuard(_brokerService);
            IDomainObjectFactory factory = new DomainObjectFactory();
            if (message.IsHeartbeat)
            {
                IEventHeartbeatDecoder decoder = new EventHeartbeatDecoder();
                IEventHeartbeat heartbeat = decoder.Decode(message.DomainObject);
                // Change heart beat information in order to confirm that we still are alive.
                _customThreadPool.QueueUserWorkItem(SendThreadedHeartbeat, heartbeat);
            }
            else
            {
                _brokerService.SendReceipt(factory.CreateReceipt(message.EventId, Process.GetCurrentProcess().Id, _userName));
            }
        }

        private void SendThreadedHeartbeat(object state)
        {
            IEventHeartbeat heartbeat = (IEventHeartbeat) state;
            heartbeat.HeartbeatId = Guid.NewGuid();
            heartbeat.SubscriberId = SubscriberId;
            heartbeat.ProcessId = Process.GetCurrentProcess().Id;
            heartbeat.ChangedBy = _userName;
            _lastHeartbeat = DateTime.Now;
            if (_brokerService==null)
            {
                return;
            }
            _brokerService.SendHeartbeat(heartbeat);
        }

        /// <summary>
        /// Called when [unhandled exception].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exceptionObject = (Exception)e.ExceptionObject;
            Log(new LogEntry(Guid.Empty, Process.GetCurrentProcess().Id, "Interprocess Message Failed.", exceptionObject.GetType().Name, exceptionObject.Message, exceptionObject.StackTrace, _userName, DateTime.Now));
        }

        /// <summary>
        /// Called when [unhandled exception handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected void OnUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception)e.ExceptionObject;
            BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "MessageBrokerBase::OnUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e). {0} {1} {2}", exc.Message, exc.StackTrace, Thread.CurrentThread.ManagedThreadId));
            if (_exceptionHandler != null)
                _exceptionHandler(sender, e);

            Debug.WriteLine(((Exception)e.ExceptionObject).Message + ((Exception)e.ExceptionObject).StackTrace);
        }

        /// <summary>
        /// Creates the addresses.
        /// </summary>
        /// <returns></returns>
        public IMessageInformation[] CreateAddresses()
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialized == 0)
            {
                ServiceGuard(_brokerService);
                return _brokerService.RetrieveAddresses();
            }
            return new IMessageInformation[0];
        }

        /// <summary>
        /// Creates the configurations.
        /// </summary>
        /// <returns></returns>
        public IConfigurationInfo[] CreateConfigurations()
        {
            if (!String.IsNullOrEmpty(_connectionString) && _initialized == 0)
            {
                ServiceGuard(_brokerService);
                return _brokerService.RetrieveConfigurations(String.Empty);
            }
            return new IConfigurationInfo[0];
        }

        /// <summary>
        /// Internals the log.
        /// </summary>
        /// <param name="exception">The exception.</param>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void InternalLog(Exception exception)
        {
            ServiceGuard(_brokerService);
            _brokerService.Log(Process.GetCurrentProcess().Id, String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", DateTime.Now, EventLogEntryType.Error, exception.Message), exception.GetType().Name, exception.Message, exception.StackTrace, _userName);
        }



        /// <summary>
        /// Services the guard.
        /// </summary>
        /// <param name="service">The service.</param>
        public void ServiceGuard(IBrokerService service)
        {
            if (service == null)
            {
                Initialized = -1;
                throw new BrokerNotInstantiatedException("Broker Service is null.");
            }
        }

        /// <summary>
        /// Internals the log.
        /// </summary>
        /// <param name="message">The message.</param>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        protected void InternalLog(string message)
        {
            ServiceGuard(BrokerService);
            BrokerService.Log(Process.GetCurrentProcess().Id, String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", DateTime.Now, EventLogEntryType.SuccessAudit, message), String.Empty, String.Empty, String.Empty, _userName);
        }

        #region IDisposable Implementation

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                StopMessageBroker();
                if (_customThreadPool != null)
                {
                    _customThreadPool.Dispose();
                    _customThreadPool = null;
                }
                if (_timer != null)
                    _timer.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            MessageBroker = null;
        }

        ~MessageBrokerBase()
        {
            Dispose(true);
        }

        #endregion
    }
}
