// ReSharper disable MemberCanBeMadeStatic
// ReSharper disable MemberCanBeMadeStatic.Local

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Messaging.Composites;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Exceptions;
using Teleopti.Messaging.Protocols;

namespace Teleopti.Messaging.Client
{
    
    /// <summary>
    /// The Message Broker Implementation.
    /// </summary>
    public sealed class MessageBrokerImplementation : MessageBrokerBase, IMessageBroker
    {
        private static readonly object _lockObject = new object();
        private readonly string _userName = Environment.UserName;
        private string channelName;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBrokerImplementation"/> class.
        /// </summary>
        private MessageBrokerImplementation()
        {
            MessageRegistrationManager = new MessageRegistrationManager(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBrokerImplementation"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        private MessageBrokerImplementation(String connectionString) : this()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBrokerImplementation"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        private MessageBrokerImplementation(IBrokerService brokerService) : this()
        {
            BrokerService = brokerService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBrokerImplementation"/> class.
        /// </summary>
        /// <param name="typeFilter">The type filter.</param>
        private MessageBrokerImplementation(IDictionary<Type, IList<Type>> typeFilter) : this()
        {
            FilterManager.InitializeTypeFilter(typeFilter);
            IsTypeFilterApplied = true;
        }


        #endregion

        #region Initialise()

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        /// <returns></returns>
        protected override Int32 Initialize()
        {
            InitializeUserWithServer();
            CreateDispatcher();
            StartSubscriber();
            StartHeartbeat();
            return 0;
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        /// <returns></returns>
        protected override Int32 InitializeUponRestart()
        {
            UnregisterClient();
            InitializeUserWithServer();
            CreateDispatcher();
            StartSubscriber();

            return 0;
        }

        /// <summary>
        /// Posts the initialize.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 25/05/2010
        /// </remarks>
        protected override void PostInitialize()
        {
            RegisterEventSubscription(OnHeartbeat, typeof(IEventHeartbeat));
            InternalLog("Client registered to listen to heartbeat events.");
        }

        private void InitializeUserWithServer()
        {
            CreateChannelForRemoteObject();
            RetrieveBrokerServiceHandle();
            RegisterUserWithBrokerService();
            RetrieveSubscriberId();
            DisposeOldSubscriber();
            ISocketInfo socketInformation = RetrieveSocketInformation();
            Subscriber = CreateSubscriber(socketInformation);
            Subscriber.UnhandledExceptionHandler += OnUnhandledExceptionHandler;
            Subscriber.EventMessageHandler += OnEventMessage;
        }

		protected override void DisposeOldSubscriber()
		{
			if (Subscriber != null)
			{
				Subscriber.UnhandledExceptionHandler -= OnUnhandledExceptionHandler;
				Subscriber.EventMessageHandler -= OnEventMessage;
				Subscriber.Dispose();
				Subscriber = null;
			}
		}

    	private void CreateDispatcher()
        {
            Dispatcher = new MessageDispatcher(this, BrokerService, FilterManager);
            MessageRegistrationManager.BrokerService = BrokerService;
            StaticData = new StaticDataManager(this);
        }

        private void StartSubscriber()
        {
            Subscriber.StartSubscribing(Threads);
            InternalLog(String.Format(CultureInfo.InvariantCulture, "Client successfully started the subscriber with port {0}.", MessagingPort));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Logging.BaseLogger.WriteLine(System.Diagnostics.EventLogEntryType,System.Type,System.String)"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void UnregisterClient()
        {
            if (string.IsNullOrEmpty(channelName)) return;

            try
            {
                var channel = ChannelServices.GetChannel(channelName);
                if (channel!=null)
                {
                    ChannelServices.UnregisterChannel(channel);
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Information, GetType(), string.Format(CultureInfo.InvariantCulture,"Successfully unregistered channel {0}.",channelName));
                }
                channelName = string.Empty;
            }
            catch (Exception exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), exc.Message + exc.StackTrace);
            }
        }

        /// <summary>
        /// Creates the channel for remote object.
        /// </summary>
        private void CreateChannelForRemoteObject()
        {
            // Create a channel for communicating with the remote object
            // Notice no port is specified on the client
            try
            {
                channelName = string.Format(CultureInfo.InvariantCulture, "MB Channel {0}", Guid.NewGuid());

                BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                serverProvider.TypeFilterLevel = TypeFilterLevel.Full;
                BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
                IDictionary properties = new Hashtable();
                properties.Add("port", 0);
                properties.Add("name", channelName);
                properties.Add("timeout", 30000);
                properties.Add("retryCount", 3);
                TcpChannel channel = new TcpChannel(properties, clientProvider, serverProvider);
                ChannelServices.RegisterChannel(channel, false);
            }
            catch (RemotingException exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), exc.Message + exc.StackTrace);
            }
        }

        /// <summary>
        /// Registers the user with broker service.
        /// </summary>
        private void RegisterUserWithBrokerService()
        {
            UserId = BrokerService.RegisterUser(Environment.UserDomainName, (_userName.Length > 10 ? _userName.Substring(0, 10) : _userName));
            InternalLog(String.Format(CultureInfo.InvariantCulture, "Client successfully retrieved User Id of {0}.", UserId));
        }

        /// <summary>
        /// Gets the subscriber id.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private void RetrieveSubscriberId()
        {
            IPAddress ipAddress = RetrieveIpAddress();
            IList<IEventSubscriber> eventSubscribers = RetrieveSubscribers(ipAddress);
            if (eventSubscribers != null && MessagingProtocol != MessagingProtocol.ClientTcpIP)
            {
                IConcurrentUsers numberOfConcurrentUsers = BrokerService.RetrieveNumberOfConcurrentUsers(ipAddress.ToString());
                MessagingPort = numberOfConcurrentUsers != null ? BrokerService.MessagingPort + numberOfConcurrentUsers.NumberOfConcurrentUsers : BrokerService.MessagingPort;
                while (CheckPortExists(eventSubscribers))
                    MessagingPort++;
            }
            else
            {
                MessagingPort = BrokerService.MessagingPort;
            }
            SubscriberId = BrokerService.RegisterSubscriber(UserId, (_userName.Length > 10 ? _userName.Substring(0, 10) : _userName), Process.GetCurrentProcess().Id, ipAddress.ToString(), MessagingPort);
            BrokerService.UpdatePortForSubscriber(SubscriberId, MessagingPort);
            InternalLog(String.Format(CultureInfo.InvariantCulture, "Client successfully  retrieved Subscriber Id of {0}.", SubscriberId));
            InternalLog(String.Format(CultureInfo.InvariantCulture, "Client with subscriber id {0} is using port {1}.", SubscriberId, MessagingPort));
        }

        /// <summary>
        /// Checks the port exists.
        /// </summary>
        /// <param name="eventSubscribers">The event subscribers.</param>
        /// <returns></returns>
        private bool CheckPortExists(IEnumerable<IEventSubscriber> eventSubscribers)
        {
            foreach (IEventSubscriber eventSubscriber in eventSubscribers)
                if(eventSubscriber.Port == MessagingPort)
                    return true;
            return false;
        }

        /// <summary>
        /// Retrieves the subscribers.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        private IList<IEventSubscriber> RetrieveSubscribers(IPAddress ipAddress)
        {
            IEventSubscriber[] subscriberArray = BrokerService.RetrieveSubscribers(ipAddress.ToString());
            if (subscriberArray != null && subscriberArray.Length > 0)
                return new List<IEventSubscriber>(subscriberArray);
            return null;
        }

        /// <summary>
        /// Retrieves the ip address and messaging port.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private IPAddress RetrieveIpAddress()
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = SocketUtility.IsIpAddress(hostEntry);
            return ipAddress;
        }

        /// <summary>
        /// Retrieves the socket information. Update default port.
        /// </summary>
        /// <returns></returns>
        private ISocketInfo RetrieveSocketInformation()
        {
            ISocketInfo socketInformation = BrokerService.RetrieveSocketInformation();
            socketInformation.Port = MessagingPort;
            return socketInformation;
        }

        /// <summary>
        /// Gets the subscriber.
        /// </summary>
        /// <param name="socketInformation">The socket information.</param>
        /// <returns></returns>
        // ReSharper disable MemberCanBeMadeStatic
        private ISubscriber CreateSubscriber(ISocketInfo socketInformation)
        {
            switch (MessagingProtocol)
            {
                case MessagingProtocol.Multicast:
                    return new Subscriber(socketInformation, new MulticastProtocol(socketInformation));
                case MessagingProtocol.TcpIP:
                    return new Subscriber(socketInformation, new TcpIpProtocol(socketInformation));
                case MessagingProtocol.Udp:
                    return new Subscriber(socketInformation, new UdpProtocol(socketInformation));
                case MessagingProtocol.ClientTcpIP:
                    return new Subscriber(socketInformation, new PollingProtocol(BrokerService, SubscriberId, socketInformation));
                default:
                    throw new BrokerNotInstantiatedException("Could not determine messaging protocol.");
            }
        }

        // ReSharper restore MemberCanBeMadeStatic


        #endregion

        #region GetInstance()

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        public static IMessageBroker GetInstance()
        {
            lock (_lockObject)
            {
                if (MessageBroker == null)
                {
                    MessageBroker = new MessageBrokerImplementation();
                }
            }
            return MessageBroker;
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
                if (MessageBroker == null)
                {
                    MessageBroker = new MessageBrokerImplementation(connectionString);
                }
            }
            return MessageBroker;
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
                if (MessageBroker == null)
                {
                    MessageBroker = new MessageBrokerImplementation(typeFilter);
                }
            }
            return MessageBroker;
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
                if (MessageBroker == null)
                {
                    MessageBroker = new MessageBrokerImplementation(brokerService);
                }
            }
            return MessageBroker;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        /// <param name="threads">The threads.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        [Obsolete("Provide only the connection string instead")]
        public static IMessageBroker GetInstance(string server, int port, int threads, string connectionString)
        {
            return GetInstance(connectionString);
        }

        #endregion

        private void OnHeartbeat(object sender, EventMessageArgs e)
        {
            // If messages comes from the BrokerService (i.e. !e.Message.IsInterprocess)
            // we need to notify the server upon recieving.
            if (!e.Message.IsInterprocess)
                SendReceipt(e.Message);
        }

        /// <summary>
        /// Called when [event message].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void OnEventMessage(object sender, EventMessageArgs e)
        {
            // Any delegate associated with this event will receive everything
            if (EventMessage != null)
                EventMessage(sender, e);

            try
            {
                var splittedType = e.Message.DomainObjectType.Split(',');
                string interfaceType = String.Format(CultureInfo.InvariantCulture, "{0}, {1}", splittedType[0], splittedType[1]);
                e.Message.InterfaceType = Type.GetType(interfaceType);
                if (e.Message.InterfaceType == null)
                    e.Message.InterfaceType = Type.GetType(e.Message.DomainObjectType);
            }
            catch (Exception exc)
            {
                BrokerService.Log(Process.GetCurrentProcess().Id, String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", DateTime.Now, EventLogEntryType.Error, exc.Message), exc.GetType().Name, exc.Message, exc.StackTrace, _userName);
            }

            // Messages comming from the BrokerService (i.e. !e.Message.IsInterprocess)
            // needs to be filtered out on Process Id and ChangedBy.
            if (e.Message.IsInterprocess ||
                (!e.Message.IsInterprocess &&
                 e.Message.ChangedBy != _userName) ||
                (!e.Message.IsInterprocess &&
                 e.Message.ChangedBy == _userName &&
                 e.Message.ProcessId != Process.GetCurrentProcess().Id))
                MessageRegistrationManager.CheckFilters(e);

            if (!e.Message.IsInterprocess && !e.Message.IsHeartbeat && e.Message.InterfaceType!=typeof(IExternalAgentState))
                SendReceipt(e.Message);
        }

        /// <summary>
        /// Sends the event message.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="referenceObjectId">The reference object id.</param>
        /// <param name="referenceObjectType">Type of the reference object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <param name="domainObject">The domain object.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        public void SendEventMessage(DateTime eventStartDate,
                                    DateTime eventEndDate,
                                    Guid moduleId,
                                    Guid referenceObjectId,
                                    Type referenceObjectType,
                                    Guid domainObjectId,
                                    Type domainObjectType,
                                    DomainUpdateType updateType,
                                    byte[] domainObject)
        {
            Dispatcher.SendEventMessage(eventStartDate, eventEndDate, moduleId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, domainObject);
        }

        /// <summary>
        /// Sends the event message.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <param name="domainObject">The domain object.</param>
        public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
        {
            Dispatcher.SendEventMessage(eventStartDate, eventEndDate, moduleId, domainObjectId, domainObjectType, domainObjectId, domainObjectType, updateType, domainObject);
        }

        /// <summary>
        /// Sends the event messages.
        /// </summary>
        /// <param name="eventMessages">The event messages.</param>
        public void SendEventMessages(IEventMessage[] eventMessages)
        {
            if(Dispatcher != null)
                Dispatcher.SendEventMessages(eventMessages);
        }

        /// <summary>
        /// Registers the event subscription.
        /// Designated method for Raptor Developers to Register Event Subscriptions
        /// passing in a delegate where no filters are applicable.
        /// </summary>
        /// <param name="eventMessageHandler">The event message handler.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// </remarks>
        public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType)
        {
            MessageRegistrationManager.RegisterEventSubscription(eventMessageHandler, domainObjectType, domainObjectType);
        }

        /// <summary>
        /// Designated method for Raptor Developers to Register Event Subscriptions
        /// passing in a delegate along with filter criterias in form of a GUID.
        /// </summary>
        /// <param name="eventMessageHandler"></param>
        /// <param name="domainObjectId"></param>
        /// <param name="domainObjectType"></param>
        public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType)
        {
            MessageRegistrationManager.RegisterEventSubscription(eventMessageHandler, domainObjectId, domainObjectType, domainObjectId, domainObjectType);
        }

        /// <summary>
        /// Registers the event subscription.
        /// </summary>
        /// <param name="eventMessageHandler">The event message handler.</param>
        /// <param name="referenceObjectId">The reference object id.</param>
        /// <param name="referenceObjectType">Type of the reference object.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 25/05/2009
        /// </remarks>
        public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType)
        {
            MessageRegistrationManager.RegisterEventSubscription(eventMessageHandler, referenceObjectId, referenceObjectType, Guid.Empty, domainObjectType);
        }

        /// <summary>
        /// Method for Raptor Developers to Register Event Subscriptions for the Scheduler,
        /// a delegate and filter criterias including dates is taken as arguments but no Guid.
        /// </summary>
        /// <param name="eventMessageHandler"></param>
        /// <param name="domainObjectType"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate)
        {
            MessageRegistrationManager.RegisterEventSubscription(eventMessageHandler, domainObjectType, domainObjectType, startDate, endDate);
        }

        /// <summary>
        /// Method for Raptor Developers to Register Event Subscriptions,
        /// a delegate and filter criterias including dates is taken as arguments.
        /// </summary>
        /// <param name="eventMessageHandler"></param>
        /// <param name="domainObjectId"></param>
        /// <param name="domainObjectType"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
        {
            MessageRegistrationManager.RegisterEventSubscription(eventMessageHandler, domainObjectId, domainObjectType, domainObjectId, domainObjectType, startDate, endDate);
        }

        /// <summary>
        /// Unregister a delegate and all filters associated will be unregistered as well.
        /// </summary>
        /// <param name="eventMessageHandler"></param>
        public void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
        {
            MessageRegistrationManager.UnregisterEventSubscription(eventMessageHandler);
        }


        /// <summary>
        /// Creates event message, ignoring dates and payload.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="domainObjectId"></param>
        /// <param name="domainObjectType"></param>
        /// <param name="updateType"></param>
        /// <returns></returns>
        public IEventMessage CreateEventMessage(Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
        {
            IDomainObjectFactory processor = new DomainObjectFactory();
            IEventMessage message = processor.CreateEventMessage(Consts.MinDate, Consts.MaxDate, UserId, Process.GetCurrentProcess().Id, moduleId, 0, false, domainObjectId, (IsTypeFilterApplied ? FilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName), domainObjectId, (IsTypeFilterApplied ? FilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName), updateType, _userName);
            return message;
        }

        /// <summary>
        /// Creates event message, ignoring dates and payload.
        /// </summary>
        /// <param name="moduleId">The module id.</param>
        /// <param name="referenceObjectId">The reference object id.</param>
        /// <param name="referenceObjectType">Type of the reference object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <returns></returns>
        public IEventMessage CreateEventMessage(Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
        {
            IDomainObjectFactory processor = new DomainObjectFactory();
            IEventMessage message = processor.CreateEventMessage(Consts.MinDate, Consts.MaxDate, UserId, Process.GetCurrentProcess().Id, moduleId, 0, false, referenceObjectId, (IsTypeFilterApplied ? FilterManager.LookupType(referenceObjectType) : referenceObjectType.AssemblyQualifiedName), domainObjectId, (IsTypeFilterApplied ? FilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName), updateType, _userName);
            return message;
        }

        /// <summary>
        /// Create Event Message, consider event range dates but not payload.
        /// </summary>
        /// <param name="eventStartDate"></param>
        /// <param name="eventEndDate"></param>
        /// <param name="moduleId"></param>
        /// <param name="domainObjectId"></param>
        /// <param name="domainObjectType"></param>
        /// <param name="updateType"></param>
        /// <returns></returns>
        public IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
        {
            IDomainObjectFactory processor = new DomainObjectFactory();
            IEventMessage message = processor.CreateEventMessage(eventStartDate, eventEndDate, UserId, Process.GetCurrentProcess().Id, moduleId, 0, false, domainObjectId, (IsTypeFilterApplied ? FilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName), domainObjectId, (IsTypeFilterApplied ? FilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName), updateType, _userName);
            return message;
        }

        /// <summary>
        /// Create Event Message, consider event range dates but not payload.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="referenceObjectId">The reference object id.</param>
        /// <param name="referenceObjectType">Type of the reference object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <returns></returns>
        public IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
        {
            IDomainObjectFactory processor = new DomainObjectFactory();
            IEventMessage message = processor.CreateEventMessage(eventStartDate, eventEndDate, UserId, Process.GetCurrentProcess().Id, moduleId, 0, false, referenceObjectId, (IsTypeFilterApplied ? FilterManager.LookupType(referenceObjectType) : referenceObjectType.AssemblyQualifiedName), domainObjectId, (IsTypeFilterApplied ? FilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName), updateType, _userName);
            return message;
        }

        #region StaticData

        /// <summary>
        /// The configuration settings of this instance of the message broker, which is received from the server.
        /// </summary>
        /// <returns></returns>
        public IList<IConfigurationInfo> RetrieveConfigurations()
        {
            return StaticData.RetrieveConfigurations();
        }

        /// <summary>
        /// The Addresses
        /// </summary>
        /// <returns></returns>
        public IList<IMessageInformation> RetrieveAddresses()
        {
            return StaticData.RetrieveAddresses();
        }

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <param name="configurations"></param>
        public void UpdateConfigurations(IList<IConfigurationInfo> configurations)
        {
            StaticData.UpdateConfigurations(configurations);
        }

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <param name="configurationInfo"></param>
        public void DeleteConfigurationItem(IConfigurationInfo configurationInfo)
        {
            StaticData.DeleteConfigurationItem(configurationInfo);
        }

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <param name="addresses"></param>
        public void UpdateAddresses(IList<IMessageInformation> addresses)
        {
            StaticData.UpdateAddresses(addresses);
        }

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <param name="addressInfo"></param>
        public void DeleteAddressItem(IMessageInformation addressInfo)
        {
            StaticData.DeleteAddressItem(addressInfo);
        }

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <returns></returns>
        public IEventHeartbeat[] RetrieveHeartbeats()
        {
            return StaticData.RetrieveHeartbeats();
        }

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <returns></returns>
        public ILogbookEntry[] RetrieveLogbookEntries()
        {
            return StaticData.RetrieveLogbookEntries();
        }

        /// <summary>
        /// Retrieves Event User Info
        /// </summary>
        /// <returns></returns>
        public IEventUser[] RetrieveEventUsers()
        {
            return StaticData.RetrieveEventUsers();
        }

        /// <summary>
        /// Retrieve Event Receipt
        /// </summary>
        /// <returns></returns>
        public IEventReceipt[] RetrieveEventReceipt()
        {
            return StaticData.RetrieveEventReceipt();
        }

        /// <summary>
        /// Retrieve Subscriber Information
        /// </summary>
        /// <returns></returns>
        public IEventSubscriber[] RetrieveSubscribers()
        {
            return StaticData.RetrieveSubscribers();
        }

        /// <summary>
        /// Retrieve Filter Information
        /// </summary>
        /// <returns></returns>
        public IEventFilter[] RetrieveFilters()
        {
            return StaticData.RetrieveFilters();
        }

        #endregion

    }
}

// ReSharper restore MemberCanBeMadeStatic
// ReSharper restore MemberCanBeMadeStatic.Local