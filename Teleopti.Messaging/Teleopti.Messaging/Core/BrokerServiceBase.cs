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
using Teleopti.Messaging.DataAccessLayer;
using Teleopti.Messaging.Events;
using Timer = System.Timers.Timer;

namespace Teleopti.Messaging.Core
{
    public abstract class BrokerServiceBase : MarshalByRefObject, IDisposable
    {
        private const string NameConstant = "TeleoptiBrokerService";
        private const string ThreadsConstant = "Threads";
        private const string IntervalConstant = "Intervall";
        private const string ConnectionStringConstant = "MessageBroker";
        private Int32 _threads;
        private CustomThreadPool _customThreadPool;
        private CustomThreadPool _databaseThreadPool;
        private CustomThreadPool _receiptThreadPool;
        private CustomThreadPool _heartbeatThreadPool;
        private readonly IPublisher _publisher;
        private Timer _timer;
        private Thread _heartbeatThread;
        private double _intervall;
        private string _connectionString;
        private int _databaseThreads;
        private int _generalThreads;
        private int _heartbeatThreads;
        private int _receiptThreads;
        private bool _alreadyDisposed;
        private readonly MessagingProtocol _protocol;
        protected static object _lockObject = new object();
        private IList<IEventSubscriber> _eventSubscriptions;

        protected BrokerServiceBase(IPublisher publisher)
        {
            _protocol = publisher.Protocol;
            _connectionString = ConfigurationManager.AppSettings["MessageBroker"];
            _publisher = publisher;
            Initialise();
        }

        private void Initialise()
        {
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
                if (configurationInfo.ConfigurationName.ToLower(CultureInfo.CurrentCulture) == ThreadsConstant.ToLower(CultureInfo.CurrentCulture))
                    _threads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.CurrentCulture);
                if (configurationInfo.ConfigurationName.ToLower(CultureInfo.CurrentCulture) == IntervalConstant.ToLower(CultureInfo.CurrentCulture))
                    _intervall = Convert.ToDouble(configurationInfo.ConfigurationValue, CultureInfo.CurrentCulture);
                if (configurationInfo.ConfigurationName.ToLower(CultureInfo.CurrentCulture) == ThreadPoolThreadSetting.DatabaseThreadPoolThreads.ToString().ToLower(CultureInfo.CurrentCulture))
                    _databaseThreads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.CurrentCulture);
                if (configurationInfo.ConfigurationName.ToLower(CultureInfo.CurrentCulture) == ThreadPoolThreadSetting.GeneralThreadPoolThreads.ToString().ToLower(CultureInfo.CurrentCulture))
                    _generalThreads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.CurrentCulture);
                if (configurationInfo.ConfigurationName.ToLower(CultureInfo.CurrentCulture) == ThreadPoolThreadSetting.HeartbeatThreadPoolThreads.ToString().ToLower(CultureInfo.CurrentCulture))
                    _heartbeatThreads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.CurrentCulture);
                if (configurationInfo.ConfigurationName.ToLower(CultureInfo.CurrentCulture) == ThreadPoolThreadSetting.ReceiptThreadPoolThreads.ToString().ToLower(CultureInfo.CurrentCulture))
                    _receiptThreads = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.CurrentCulture);

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

                InternalInformationLog(String.Format("Teleopti is using a {0} ...", _publisher.GetType()));
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
            // Insert receipt and heart beat. Write to teleopti event log.
            IEventReceipt receipt = (IEventReceipt) state;
            IBrokerProcessor processor = GetProcessor();
            processor.InsertEventReceipt(receipt);
        }

        protected void AcceptHeartbeat(object state)
        {
            IEventHeartbeat beat = (IEventHeartbeat)state;
            IBrokerProcessor processor = GetProcessor();
            processor.InsertEventHeartbeat(beat);
        }

        private void OnUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception) e.ExceptionObject;
            BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.CurrentCulture, "BrokerServiceBase::OnUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e). {0}{1}", exc.Message, exc.StackTrace));            
            IBrokerProcessor processor = GetProcessor();
            processor.InsertEventLogEntry(Process.GetCurrentProcess().Id, "Unhandled exception on background thread", e.ExceptionObject.GetType().ToString(), exc.Message, exc.StackTrace, Environment.UserName);
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
            IBrokerProcessor processor = new BrokerProcessor(_connectionString);
            processor.RunScavageEvents();
            IEventHeartbeat heartbeat = new EventHeartbeat(Guid.NewGuid(), Process.GetCurrentProcess().Id, Environment.UserName, DateTime.Now);
            HeartbeatInserter inserter = new HeartbeatInserter(_connectionString);
            inserter.Execute(heartbeat);
            IEventMessage eventMessage = processor.CreateEventMessage(DateTime.Now, DateTime.Now, 1, Process.GetCurrentProcess().Id, Guid.Empty, 0, true, Guid.Empty, typeof(EventHeartbeat).AssemblyQualifiedName, DomainUpdateType.Update, Environment.UserName);
            IEventHeartbeatEncoder encoder = new EventHeartbeatEncoder();
            byte[] encoded = encoder.Encode(heartbeat);
            eventMessage.DomainObject = encoded;
            if(_protocol == MessagingProtocol.TCPIP)
            {
                List<IMessageInfo> messages = GetMessages(eventMessage);
                ((ITcpIpPublisher)_publisher).Send(messages);
            }
            else
            {
                ((IMulticastPublisher)_publisher).Send(eventMessage);
            }
        }

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

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected BrokerProcessor GetProcessor()
        {
            return new BrokerProcessor(_connectionString);
        }

        protected void SendAsync(object state)
        {
            IEventMessage eventMessage = (IEventMessage)state;
            _databaseThreadPool.QueueUserWorkItem(InsertObjectAsync, eventMessage);
            if (_protocol == MessagingProtocol.TCPIP)
            {
                IList<IMessageInfo> messages = GetMessages(eventMessage);
                ((ITcpIpPublisher)_publisher).Send(messages);
            }
            else
            {
                ((IMulticastPublisher)_publisher).Send(eventMessage);
            }
        }

        protected void SendAsyncList(object state)
        {
            IList<IEventMessage> eventMessages = (IList<IEventMessage>)state;
            _databaseThreadPool.QueueUserWorkItem(InsertObjectAsync, eventMessages);
            if (_protocol == MessagingProtocol.TCPIP)
            {
                IList<IMessageInfo> messages = GetMessages(eventMessages);
                ((ITcpIpPublisher)_publisher).Send(messages);
            }
            else
            {
                ((IMulticastPublisher)_publisher).Send(eventMessages);
            }
        }

        private IList<IMessageInfo> GetMessages(IEnumerable<IEventMessage> eventMessages)
        {
            List<IMessageInfo> messages = new List<IMessageInfo>();
            foreach (IEventMessage eventMessage in eventMessages)
                messages.AddRange(GetMessages(eventMessage));
            return messages;
        }
            
        private List<IMessageInfo> GetMessages(IEventMessage eventMessage)
        {
            List<IMessageInfo> messages = new List<IMessageInfo>();
            lock (_lockObject)
            {
                foreach (IEventSubscriber eventSubscriber in _eventSubscriptions)
                {
                    IMessageInfo info = new MessageInfo();
                    info.Address = eventSubscriber.IPAddress;
                    info.Port = eventSubscriber.Port;
                    info.EventMessage = eventMessage;
                    messages.Add(info);
                }   
            }
            return messages;
        }

        protected void LogAsync(object state)
        {
            ILogEntry eventLogEntry = (ILogEntry) state;
            LogEntryInserter inserter = new LogEntryInserter(_connectionString);
            inserter.Execute(eventLogEntry);
        }

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

        #region IDisposable Implementation

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (!_alreadyDisposed)
                {
                    if (_customThreadPool != null)
                        _customThreadPool.Dispose();
                    if (_receiptThreadPool != null)
                        _receiptThreadPool.Dispose();
                    if (_heartbeatThreadPool != null)
                        _heartbeatThreadPool.Dispose();
                    if (_publisher != null)
                        _publisher.Dispose();
                    if (_databaseThreadPool != null)
                        _databaseThreadPool.Dispose();
                    if (_timer != null)
                        _timer.Dispose();

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
