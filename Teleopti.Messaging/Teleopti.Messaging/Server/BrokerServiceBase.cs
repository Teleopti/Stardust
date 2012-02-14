using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Permissions;
using System.Threading;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Logging.Core;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Composites;
using Teleopti.Messaging.DataAccessLayer;
using Teleopti.Messaging.Events;
using Timer = System.Timers.Timer;

namespace Teleopti.Messaging.Server
{
    public abstract class BrokerServiceBase : MarshalByRefObject, IDisposable
    {
        private const string NameConstant = "TeleoptiBrokerService";
        private const string ThreadsConstant = "Threads";
        private const string IntervalConstant = "Intervall";
        private const string ConnectionStringConstant = "MessageBroker";
        private const string ClientThrottleConstant = "ClientThrottle";

        private CustomThreadPool _customThreadPool;
        private CustomThreadPool _databaseThreadPool;
        private CustomThreadPool _receiptThreadPool;
        private CustomThreadPool _heartbeatThreadPool;
        private IPublisher _publisher;
        private Timer _timer;
        private Thread _heartbeatThread;
        private double _intervall;
        private string _connectionString;
        private int _databaseThreads;
        private int _generalThreads;
        private int _heartbeatThreads;
        private int _receiptThreads;
        private int _messagingPort;
        private string _multicastAddress;
        private bool _alreadyDisposed;
        private MessagingProtocol _protocol;
        private IList<IEventSubscriber> _eventSubscriptions = new List<IEventSubscriber>();
        private readonly IDictionary<Guid, IList<IEventFilter>> _filters = new Dictionary<Guid, IList<IEventFilter>>();
        private int _clientThrottle;
        private long _restartTime = 2;
        private Int32 _threads;
        private int _timeToLive;

        // ReSharper restore DoNotCallOverridableMethodsInConstructor
        protected abstract void PopulatePublisher(IPublisher publisher);

        /// <summary>
        /// Initialises the specified publisher.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        /// <param name="connectionString">The connection string.</param>
        public void Initialize(IPublisher publisher, string connectionString)
        {
            _protocol = publisher.Protocol;
            _connectionString = connectionString;
            _publisher = publisher;
            PopulatePublisher(publisher);
            InitialiseFromDatabase();  
            InitialiseThreadPools();
        }

        private void InitialiseFromDatabase()
        {
            if (String.IsNullOrEmpty(_connectionString))
                _connectionString = ConfigurationManager.AppSettings[ConnectionStringConstant]; 
            ConfigurationInfoReader reader = new ConfigurationInfoReader(_connectionString);
            IList<IConfigurationInfo> configurationInfos = reader.Execute();
            foreach (IConfigurationInfo configurationInfo in configurationInfos)
                SetConfigurationInfo(configurationInfo);
        }

        private void SetConfigurationInfo(IConfigurationInfo configurationInfo)
        {
            if (configurationInfo.ConfigurationType == NameConstant)
            {
                if (configurationInfo.ConfigurationName.ToUpperInvariant() == ThreadsConstant.ToUpperInvariant())
                    _threads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                if (configurationInfo.ConfigurationName.ToUpperInvariant() == IntervalConstant.ToUpperInvariant())
                    _intervall = Convert.ToDouble(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                if (configurationInfo.ConfigurationName.ToUpperInvariant() == ThreadPoolThreadSetting.DatabaseThreadPoolThreads.ToString().ToUpperInvariant())
                    _databaseThreads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                if (configurationInfo.ConfigurationName.ToUpperInvariant() == ThreadPoolThreadSetting.GeneralThreadPoolThreads.ToString().ToUpperInvariant())
                    _generalThreads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                if (configurationInfo.ConfigurationName.ToUpperInvariant() == ThreadPoolThreadSetting.HeartbeatThreadPoolThreads.ToString().ToUpperInvariant())
                    _heartbeatThreads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                if (configurationInfo.ConfigurationName.ToUpperInvariant() == ThreadPoolThreadSetting.ReceiptThreadPoolThreads.ToString().ToUpperInvariant())
                    _receiptThreads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                if (configurationInfo.ConfigurationName.ToUpperInvariant() == ClientThrottleConstant.ToUpperInvariant())
                    _clientThrottle = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                if (configurationInfo.ConfigurationName.ToUpperInvariant() == Client.MessageBrokerBase.RestartTimeConstant.ToUpperInvariant())
                    _restartTime = Convert.ToInt64(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), 
         SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void InitialiseThreadPools()
        {
            try
            {
                InternalInformationLog("BrokerServiceBase::Initialise()");
                InternalInformationLog("Starting Broker Service general thread pool ...");

                _customThreadPool = new CustomThreadPool(_generalThreads, ThreadPoolThreadSetting.GeneralThreadPoolThreads.ToString());

                InternalInformationLog("Starting Broker Service database thread pool ...");

                _databaseThreadPool = new CustomThreadPool(_databaseThreads, ThreadPoolThreadSetting.DatabaseThreadPoolThreads.ToString());
                
                InternalInformationLog("Starting Broker Service receipts thread pool ...");

                _receiptThreadPool = new CustomThreadPool(_receiptThreads, ThreadPoolThreadSetting.ReceiptThreadPoolThreads.ToString());

                InternalInformationLog("Starting Broker Service heartbeats thread pool ...");

                _heartbeatThreadPool = new CustomThreadPool(_heartbeatThreads, ThreadPoolThreadSetting.HeartbeatThreadPoolThreads.ToString());

                InternalInformationLog("Creating publisher ...");

                _publisher.UnhandledExceptionHandler += OnUnhandledExceptionHandler;
                _publisher.StartPublishing();

                InternalInformationLog(String.Format(CultureInfo.InvariantCulture, "Teleopti is using the {0} protocol.", Protocol));
                InternalInformationLog("Creating heart beat thread");

                _heartbeatThread = new Thread(StartHeartbeatLoop);
                _heartbeatThread.IsBackground = true;
                _heartbeatThread.Name = ThreadPoolThreadSetting.HeartbeatThreadPoolThreads.ToString();
                _heartbeatThread.Start();

            }
            catch (Exception exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.Message + exc.StackTrace);
            }
        }

        protected void InternalInformationLog(string message)
        {
            BaseLogger.Instance.WriteLine(EventLogEntryType.Information, GetType(), message);
        }

        protected void InternalErrorLog(string message)
        {
            BaseLogger.Instance.WriteLine(EventLogEntryType.Information, GetType(), message);
        }

        protected void AcceptReceipt(object state)
        {
            // Insert receipt and heartbeat. Write to teleopti event log.
            IEventReceipt receipt = (IEventReceipt) state;
            IDataMapper mapper = new DataMapper(_connectionString, _restartTime);
            mapper.InsertEventReceipt(receipt);
        }

        protected void AcceptHeartbeat(object state)
        {
            IEventHeartbeat beat = (IEventHeartbeat)state;
            if (beat.ChangedDateTime == DateTime.MinValue)
                beat.ChangedDateTime = DateTime.Now;
            IDataMapper mapper = new DataMapper(_connectionString, _restartTime);
            mapper.InsertEventHeartbeat(beat);
        }

        private void OnUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception) e.ExceptionObject;
            BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "BrokerServiceBase::OnUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e). {0} {1} {2}", exc.Message, exc.StackTrace, Thread.CurrentThread.ManagedThreadId));            
            IDataMapper mapper = new DataMapper(_connectionString, _restartTime);
            mapper.InsertEventLogEntry(Process.GetCurrentProcess().Id, "Unhandled exception on background thread", e.ExceptionObject.GetType().ToString(), exc.Message, exc.StackTrace, Environment.UserName);
            Debug.WriteLine(((Exception)e.ExceptionObject).Message + ((Exception)e.ExceptionObject).StackTrace);
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        private void StartHeartbeatLoop()
        {
            _timer = new Timer();
            _timer.Interval = _intervall;
            _timer.AutoReset = true;
            _timer.Elapsed += RunHeartbeat;
            _timer.Enabled = true;
            _timer.Start();
        }

        private void RunHeartbeat(object sender, System.Timers.ElapsedEventArgs e)
        {
            IDataMapper mapper = new DataMapper(_connectionString, _restartTime);
            mapper.RunScavenge();
            IEventHeartbeat heartbeat = new EventHeartbeat(Guid.NewGuid(), Guid.Empty, Process.GetCurrentProcess().Id, Environment.UserName, DateTime.Now);
            HeartbeatInserter inserter = new HeartbeatInserter(_connectionString);
            inserter.Execute(heartbeat);
            IDomainObjectFactory domainObjectFactory = new DomainObjectFactory();
            IEventMessage eventMessage = domainObjectFactory.CreateEventMessage(DateTime.Now, DateTime.Now, 1, Process.GetCurrentProcess().Id, Guid.Empty, 0, true, Guid.Empty, typeof(EventHeartbeat).AssemblyQualifiedName, Guid.Empty, typeof(EventHeartbeat).AssemblyQualifiedName, DomainUpdateType.Update, Environment.UserName);
            IEventHeartbeatEncoder encoder = new EventHeartbeatEncoder();
            byte[] encoded = encoder.Encode(heartbeat);
            eventMessage.DomainObject = encoded;
            List<IMessageInformation> messages = CreateMessageInformationList(eventMessage);
            _publisher.Send(messages);
            IDictionary<Guid, IEventHeartbeat> dictionary = mapper.RetrieveDistinctHeartbeats();
            RemoveStaleSubscribers(dictionary);   
        }

        protected abstract void RemoveStaleSubscribers(IDictionary<Guid, IEventHeartbeat> dictionary);

        private void InsertObjectAsync(object obj)
        {
            if (typeof(IEventMessage).IsAssignableFrom(obj.GetType()))
            {
                EventMessageInserter inserter = new EventMessageInserter(_connectionString);
                inserter.Execute((IEventMessage)obj);
            }
            if (typeof(IList<IEventMessage>).IsAssignableFrom(obj.GetType()))
            {
                EventMessageInserter inserter = new EventMessageInserter(_connectionString);
                inserter.Execute((IList<IEventMessage>)obj);
            }
        }

        #region Protected Methods

        #pragma warning disable 1692

        protected void SendAsync(object state)
        {
            IEventMessage eventMessage = (IEventMessage)state;
            _databaseThreadPool.QueueUserWorkItem(InsertObjectAsync, eventMessage);
            IList<IMessageInformation> messages = CreateMessageInformationList(eventMessage);
            _publisher.Send(messages);    
        }

        protected void SendAsyncList(object state)
        {
            IList<IEventMessage> eventMessages = (IList<IEventMessage>)state;
            _databaseThreadPool.QueueUserWorkItem(InsertObjectAsync, eventMessages);
            IList<IMessageInformation> messages = GetMessages(eventMessages);
            _publisher.Send(messages);
        }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <param name="eventMessages">The event messages.</param>
        /// <returns></returns>
        private IList<IMessageInformation> GetMessages(IEnumerable<IEventMessage> eventMessages)
        {
            List<IMessageInformation> messages = new List<IMessageInformation>();
            foreach (IEventMessage eventMessage in eventMessages)
            {
                List<IMessageInformation> messageInfos = CreateMessageInformationList(eventMessage);
                if(messageInfos.Count > 0)
                    messages.AddRange(messageInfos);
            }
            return messages;
        }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        protected abstract List<IMessageInformation> CreateMessageInformationList(IEventMessage eventMessage);

        protected void LogAsync(object state)
        {
            ILogEntry eventLogEntry = (ILogEntry) state;
            LogEntryInserter inserter = new LogEntryInserter(_connectionString);
            inserter.Execute(eventLogEntry);
        }

        /// <summary>
        /// Gets the subscriber.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        protected IEventSubscriber GetSubscriber(Guid subscriberId)
        {
            IEventSubscriber subscriber = null;
            foreach (IEventSubscriber eventSubscriber in EventSubscriptions)
            {
                if (eventSubscriber.SubscriberId == subscriberId)
                {
                    subscriber = eventSubscriber;
                    break;
                }
            }
            return subscriber;
        }

        /// <summary>
        /// Gets the subscriber.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        protected IEventSubscriber GetSubscriber(string address, int port)
        {
            IEventSubscriber subscriber = null;
            foreach (IEventSubscriber eventSubscriber in EventSubscriptions)
            {
                if (eventSubscriber.IPAddress == address && eventSubscriber.Port == port)
                {
                    subscriber = eventSubscriber;
                    break;
                }
            }
            return subscriber;
        }

        #pragma warning restore 1692

        #endregion

        /// <summary>
        /// Gets the threads.
        /// </summary>
        /// <value>The threads.</value>
        public int Threads
        {
            get { return _threads; }
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        /// Gets the heartbeat thread pool.
        /// </summary>
        /// <value>The heartbeat thread pool.</value>
        public CustomThreadPool HeartbeatThreadPool
        {
            get { return _heartbeatThreadPool; }
        }

        /// <summary>
        /// Gets the receipt thread pool.
        /// </summary>
        /// <value>The receipt thread pool.</value>
        public CustomThreadPool ReceiptThreadPool
        {
            get { return _receiptThreadPool; }
        }

        /// <summary>
        /// Gets the database thread pool.
        /// </summary>
        /// <value>The database thread pool.</value>
        public CustomThreadPool DatabaseThreadPool
        {
            get { return _databaseThreadPool; }
        }

        /// <summary>
        /// Gets the custom thread pool.
        /// </summary>
        /// <value>The custom thread pool.</value>
        public CustomThreadPool CustomThreadPool
        {
            get { return _customThreadPool; }
        }

        /// <summary>
        /// Gets or sets the event subscriptions.
        /// </summary>
        /// <value>The event subscriptions.</value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<IEventSubscriber> EventSubscriptions
        {
            get { return _eventSubscriptions; }
            set { _eventSubscriptions = value; }
        }

        /// <summary>
        /// Gets the protocol.
        /// </summary>
        /// <value>The protocol.</value>
        public MessagingProtocol Protocol
        {
            get { return _protocol; }
        }

        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>The filters.</value>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IDictionary<Guid, IList<IEventFilter>> Filters
        {
            get { return _filters; }
        }

        /// <summary>
        /// Gets the client throttle.
        /// </summary>
        /// <value>The client throttle.</value>
        public int ClientThrottle
        {
            get { return _clientThrottle; }
        }

        /// <summary>
        /// Gets or sets the messaging port.
        /// </summary>
        /// <value>The messaging port.</value>
        public int MessagingPort
        {
            get { return _messagingPort; }
            set { _messagingPort = value; }
        }

        /// <summary>
        /// Gets or sets the multicast address.
        /// </summary>
        /// <value>The multicast address.</value>
        public string MulticastAddress
        {
            get { return _multicastAddress; }
            set { _multicastAddress = value; }
        }

        /// <summary>
        /// Gets or sets the time to live.
        /// </summary>
        /// <value>The time to live.</value>
        public int TimeToLive
        {
            get { return _timeToLive; }
            set { _timeToLive = value; }
        }

        /// <summary>
        /// Gets the restart time.
        /// </summary>
        /// <value>The restart time.</value>
        protected long RestartTime
        {
            get
            {
                return _restartTime;
            }
        }

        #region IDisposable Implementation

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (!_alreadyDisposed)
                {
                    if (_customThreadPool != null)
                    {
                        _customThreadPool.Dispose();
                        _customThreadPool = null;
                    }
                    if (_receiptThreadPool != null)
                    {
                        _receiptThreadPool.Dispose();
                        _receiptThreadPool = null;
                    }
                    if (_heartbeatThreadPool != null)
                    {
                        _heartbeatThreadPool.Dispose();
                        _heartbeatThreadPool = null;
                    }
                    if (_publisher != null)
                    {
                        _publisher.Dispose();
                        _publisher = null;
                    }
                    if (_databaseThreadPool != null)
                    {
                        _databaseThreadPool.Dispose();
                        _databaseThreadPool = null;
                    }
                    if (_timer != null)
                    {
                        _timer.Dispose();
                        _timer = null;
                    }
                    if (_heartbeatThread != null)
                    {
                        _heartbeatThread.Interrupt();
                        _heartbeatThread = null;
                    }

                    _eventSubscriptions.Clear();
                    _filters.Clear();

                    _alreadyDisposed = true;

                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}