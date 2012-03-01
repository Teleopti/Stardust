using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Composites;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.DataAccessLayer;

namespace Teleopti.Messaging.Server
{
    /// <summary>
    /// The Broker Service.
    /// </summary>
    public class BrokerService : BrokerServiceBase, IBrokerService
    {
        private static readonly object _lockObject = new object();
        private readonly IDictionary<Guid, IList<IEventSubscriber>> _staleSubscribers = new Dictionary<Guid, IList<IEventSubscriber>>();

        /// <summary>
        /// Populates the publisher.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        protected override void PopulatePublisher(IPublisher publisher)
        {
            publisher.Broker = this;
        }

        /// <summary>
        /// Registers the user.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public Int32 RegisterUser(string domain, string userName)
        {
            IDomainObjectFactory domainObjectFactory = new DomainObjectFactory();
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            IEventUser user = domainObjectFactory.CreateUser(domain, userName);
            return mapper.RegisterUser(user);
        }

        /// <summary>
        /// Registers the subscriber.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="ipAddress">The IP Address of the subscriber.</param>
        /// <param name="port">The port number.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public Guid RegisterSubscriber(Int32 userId, string userName, Int32 processId, string ipAddress, int port)
        {
            IDomainObjectFactory domainObjectFactory = new DomainObjectFactory();
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            IEventSubscriber subscriber = domainObjectFactory.CreateSubscription(userId, processId, userName, ipAddress, port);
            mapper.RegisterSubscription(subscriber);
            lock (_lockObject)
            {
                EventSubscriptions.Add(subscriber);
                if (Protocol == MessagingProtocol.ClientTcpIP)
                    ThreadPool.QueueUserWorkItem(AddClient, subscriber);
            }
            return subscriber.SubscriberId;
        }

        /// <summary>
        /// Adds the client for a client tcp ip connection.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/05/2010
        /// </remarks>
        private void AddClient(object state)
        {
            IEventSubscriber subscriber = (IEventSubscriber)state;
            if (subscriber != null)
                PollingManager.Instance.AddClient(subscriber.SubscriberId);
        }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        protected override List<IMessageInformation> CreateMessageInformationList(IEventMessage eventMessage)
        {
            List<IMessageInformation> messages = new List<IMessageInformation>();
            lock (_lockObject)
            {
                foreach (IEventSubscriber eventSubscriber in EventSubscriptions)
                {
                    IList<IEventFilter> filters;
                    if (Filters.TryGetValue(eventSubscriber.SubscriberId, out filters))
                    {
                        foreach (IEventFilter eventFilter in filters)
                        {
                            IEventMessage message = CheckFilter(eventFilter, eventMessage);
                            if (message != null)
                            {
                                IMessageInformation info = new MessageInformation();
                                if (Protocol == MessagingProtocol.Multicast)
                                {
                                    // If multicast protocol only one consumer 
                                    // needs to be interested in the event message for it to be sent.
                                    // We use the multicast address but the same port.
                                    // we need to specify time to live, router hops.
                                    info.Address = MulticastAddress;
                                    info.Port = MessagingPort;
                                    info.TimeToLive = TimeToLive;
                                    info.EventMessage = eventMessage;
                                    messages.Add(info);
                                    // Return it since someone wants it.
                                    return messages;
                                }
                                // Otherwise, not multicast, break this loop of filters 
                                // and check if other event subscribers 
                                // are interested.
                                info.SubscriberId = eventSubscriber.SubscriberId;
                                info.Address = eventSubscriber.IPAddress;
                                info.Port = eventSubscriber.Port;
                                info.EventMessage = eventMessage;
                                messages.Add(info);
                                break;
                            }
                        }
                    }
                    else if (eventMessage.IsHeartbeat)
                    {
                        IMessageInformation info = new MessageInformation();
                        if (Protocol == MessagingProtocol.Multicast)
                        {
                            // If multicast protocol only one consumer 
                            // needs to be interested in the event message.
                            info.Address = MulticastAddress;
                            info.Port = MessagingPort;
                            info.TimeToLive = TimeToLive;
                            info.EventMessage = eventMessage;
                            messages.Add(info);
                            return messages;
                        }
                        // Otherwise add a heartbeat for each 
                        // unicast event subscribers that are interested.
                        info.SubscriberId = eventSubscriber.SubscriberId;
                        info.Address = eventSubscriber.IPAddress;
                        info.Port = eventSubscriber.Port;
                        info.EventMessage = eventMessage;
                        messages.Add(info);
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// Checks the filter server side.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="eventMessage">The event message.</param>
        public IEventMessage CheckFilter(IEventFilter filter, IEventMessage eventMessage)
        {
            Type filterType = filter.DomainObjectTypeCache;
            Type eventMessageType = eventMessage.DomainObjectTypeCache;
            if ((filter.DomainObjectId == eventMessage.DomainObjectId &&
                 filter.DomainObjectType != null &&
                 filterType.IsAssignableFrom(eventMessageType)) ||
                (filter.DomainObjectId == Guid.Empty &&
                 filter.DomainObjectType != null &&
                 filterType.IsAssignableFrom(eventMessageType)))
            {
                return CheckDates(filter, eventMessage);
            }
            return null;
        }


        /// <summary>
        /// Checks the date server side.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="eventMessage">The event message.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IEventMessage CheckDates(IEventFilter filter, IEventMessage eventMessage)
        {
            if (filter.EventStartDate != Consts.MinDate || filter.EventEndDate != Consts.MaxDate)
            {
                if (filter.EventStartDate <= eventMessage.EventStartDate &&
                    filter.EventEndDate >= eventMessage.EventEndDate)
                    return eventMessage;
            }
            else
            {
                return eventMessage;
            }
            return null;
        }

        /// <summary>
        /// Removes the stale subscribers.
        /// If our distinct dictionary of subscribers answering heartbeats
        /// does not contain a subscriber id in EventSubscriptions mark it as stale.
        /// If a subscriber has been stale for 3 heart beat series then remove the really stale
        /// subscriber. If heartbeats for a new client has start to arrive,
        /// it is not stale, and hence remove from list of stale subscribers.
        /// Finally if we already removed the stale subscriber,
        /// we do not need to know about it and the subscriber id will be removed
        /// from stale subscribers.
        /// </summary>
        /// <param name="dictionary">The dictionary of distinct heartbeats.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-04-05
        /// </remarks>
        protected override void RemoveStaleSubscribers(IDictionary<Guid, IEventHeartbeat> dictionary)
        {
            lock (_lockObject)
            {

                IList<Guid> oldSubscriberIds = new List<Guid>();

                // If our distinct dictionary of subscribers answering heartbeats
                // does not contain a subscriber id in EventSubscriptions 
                // mark it as stale.
                foreach (IEventSubscriber eventSubscriber in EventSubscriptions)
                {
                    if (!dictionary.ContainsKey(eventSubscriber.SubscriberId))
                    {
                        if (!_staleSubscribers.ContainsKey(eventSubscriber.SubscriberId))
                            _staleSubscribers.Add(eventSubscriber.SubscriberId, new List<IEventSubscriber>());
                        _staleSubscribers[eventSubscriber.SubscriberId].Add(eventSubscriber);
                    }
                }

                // if a subscriber been stale for 3 heart beat series then
                // remove the really stale subscribers.
                foreach (KeyValuePair<Guid, IList<IEventSubscriber>> keyValuePair in _staleSubscribers)
                {
                    if (keyValuePair.Value.Count > 3)
                    {
                        if (keyValuePair.Value[0] != null && EventSubscriptions.Contains(keyValuePair.Value[0]))
                        {
                            EventSubscriptions.Remove(keyValuePair.Value[0]);
                            Filters.Remove(keyValuePair.Key);
                            oldSubscriberIds.Add(keyValuePair.Key);
                            if (Protocol == MessagingProtocol.ClientTcpIP)
                                PollingManager.Instance.RemoveClient(keyValuePair.Key);
                        }
                    }
                }

                // If heartbeats for a new client has start to arrive, it is not stale, 
                // remove from list of stale subscribers.
                foreach (KeyValuePair<Guid, IEventHeartbeat> keyValuePair in dictionary)
                    if (_staleSubscribers.ContainsKey(keyValuePair.Key))
                        _staleSubscribers.Remove(keyValuePair.Key);


                // If we already removed the stale subscriber we do not need to know about it.
                foreach (Guid oldSubscriberId in oldSubscriberIds)
                    if (_staleSubscribers.ContainsKey(oldSubscriberId))
                        _staleSubscribers.Remove(oldSubscriberId);


            }
        }

        /// <summary>
        /// Unregisters the subscriber.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public void UnregisterSubscriber(Guid subscriberId)
        {
            lock (_lockObject)
            {
                IEventSubscriber subscriber = GetSubscriber(subscriberId);
                RemoveSubscriber(subscriber);
            }
        }

        /// <summary>
        /// Unregisters the subscriber.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public void UnregisterSubscriber(string address, int port)
        {
            lock (_lockObject)
            {
                IEventSubscriber subscriber = GetSubscriber(address, port);
                RemoveSubscriber(subscriber);
            }
        }

        /// <summary>
        /// Removes the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        private void RemoveSubscriber(IEventSubscriber subscriber)
        {
            if (subscriber != null)
            {
                IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
                EventSubscriptions.Remove(subscriber);
                IList<IEventFilter> foundFilters;
                if (Filters.TryGetValue(subscriber.SubscriberId,out foundFilters))
                {
                    foreach (IEventFilter eventFilter in foundFilters)
                        mapper.UnregisterFilter(eventFilter.FilterId);
                }
                Filters.Remove(subscriber.SubscriberId);
                mapper.UnregisterSubscription(subscriber.SubscriberId);
                if (Protocol == MessagingProtocol.ClientTcpIP)
                    PollingManager.Instance.RemoveClient(subscriber.SubscriberId);
            }
        }

        /// <summary>
        /// Updates the port for subscriber.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="port">The port.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        public void UpdatePortForSubscriber(Guid subscriberId, int port)
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            mapper.UpdatePortForSubscriber(subscriberId, port);
            lock (_lockObject)
            {
                foreach (IEventSubscriber eventSubscription in EventSubscriptions)
                {
                    if (eventSubscription.SubscriberId == subscriberId)
                    {
                        eventSubscription.Port = port;
                    }
                }
            }
        }


        /// <summary>
        /// Registers the filter.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public IEventFilter RegisterFilter(Guid subscriberId, Guid domainObjectId, string domainObjectType, DateTime startDate, DateTime endDate, string userName)
        {
            return RegisterFilter(subscriberId, domainObjectId, domainObjectType, domainObjectId, domainObjectType, startDate, endDate, userName);
        }

        /// <summary>
        /// Registers the filter.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="referenceObjectId">The reference object id.</param>
        /// <param name="referenceObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public IEventFilter RegisterFilter(Guid subscriberId, Guid referenceObjectId, string referenceObjectType, Guid domainObjectId, string domainObjectType, DateTime startDate, DateTime endDate, string userName)
        {
            IEventFilter eventFilter;
            lock (_lockObject)
            {
                IDomainObjectFactory factory = new DomainObjectFactory();
                IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
                IEventFilter filter = factory.CreateFilter(subscriberId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, startDate, endDate, userName);

                IList<IEventFilter> eventFilters;
                if (Filters.TryGetValue(subscriberId, out eventFilters))
                {
                    eventFilters.Add(filter);
                }
                else
                {
                    var list = new List<IEventFilter>{filter};
                    Filters.Add(subscriberId, list);
                }
                eventFilter = mapper.RegisterFilter(filter);
            }
            return eventFilter;
        }

        /// <summary>
        /// Unregisters the filter.
        /// </summary>
        /// <param name="filterId">The filter id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public void UnregisterFilter(Guid filterId)
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            mapper.UnregisterFilter(filterId);
        }

        /// <summary>
        /// Sends the event message.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="packageSize">Size of the package.</param>
        /// <param name="isHeartbeat">if set to <c>true</c> [is heartbeat].</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <param name="userName">Name of the user.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        public void SendEventMessage(DateTime eventStartDate,
                                        DateTime eventEndDate,
                                        Int32 userId,
                                        Int32 processId,
                                        Guid moduleId,
                                        Int32 packageSize,
                                        bool isHeartbeat,
                                        Guid domainObjectId,
                                        string domainObjectType,
                                        DomainUpdateType updateType,
                                        string userName)
        {
            IDomainObjectFactory factory = new DomainObjectFactory();
            IEventMessage eventMessage = factory.CreateEventMessage(eventStartDate, eventEndDate, userId, processId, moduleId, packageSize, isHeartbeat, domainObjectId, domainObjectType, domainObjectId, domainObjectType, updateType, userName);
            CustomThreadPool.QueueUserWorkItem(SendAsync, eventMessage);
        }

        /// <summary>
        /// Sends the event message.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="packageSize">Size of the package.</param>
        /// <param name="isHeartbeat">if set to <c>true</c> [is heartbeat].</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <param name="domainObject">The domain object.</param>
        /// <param name="userName">Name of the user.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, Int32 userId, Int32 processId, Guid moduleId, Int32 packageSize, bool isHeartbeat, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, byte[] domainObject, string userName)
        {
            IDomainObjectFactory factory = new DomainObjectFactory();
            IEventMessage eventMessage = factory.CreateEventMessage(eventStartDate, eventEndDate, userId, processId, moduleId, packageSize, isHeartbeat, domainObjectId, domainObjectType, domainObjectId, domainObjectType, updateType, domainObject, userName);
            CustomThreadPool.QueueUserWorkItem(SendAsync, eventMessage);
        }

        /// <summary>
        /// Sends the event messages.
        /// </summary>
        /// <param name="eventMessages">The event messages.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public void SendEventMessages(IEventMessage[] eventMessages)
        {
            CustomThreadPool.QueueUserWorkItem(SendAsyncList, eventMessages);
        }

        /// <summary>
        /// Logs the specified process id.
        /// </summary>
        /// <param name="processId">The process id.</param>
        /// <param name="description">The description.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="stackTrace">The stack trace.</param>
        /// <param name="userName">Name of the user.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public void Log(Int32 processId, string description, string exception, string message, string stackTrace, string userName)
        {
            IDomainObjectFactory factory = new DomainObjectFactory();
            ILogEntry eventLogEntry = factory.CreateEventLogEntry(processId, description, exception, message, stackTrace, userName);
            if (CustomThreadPool != null)
                CustomThreadPool.QueueUserWorkItem(LogAsync, eventLogEntry);
        }

        /// <summary>
        /// Retrieves the configurations.
        /// </summary>
        /// <param name="configurationType">Type of the configuration.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public IConfigurationInfo[] RetrieveConfigurations(string configurationType)
        {
            IDataMapper dataMapper = new DataMapper(ConnectionString, RestartTime);
            return dataMapper.ReadConfigurationInfo(configurationType);
        }

        /// <summary>
        /// Sends the receipt.
        /// </summary>
        /// <param name="receipt">The receipt.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public void SendReceipt(IEventReceipt receipt)
        {
            ReceiptThreadPool.QueueUserWorkItem(AcceptReceipt, receipt);
        }

        /// <summary>
        /// Sends the heartbeat.
        /// </summary>
        /// <param name="beat">The beat.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public void SendHeartbeat(IEventHeartbeat beat)
        {
            HeartbeatThreadPool.QueueUserWorkItem(AcceptHeartbeat, beat);
        }

        /// <summary>
        /// Reads the number of concurrent users for one IP Address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        public IConcurrentUsers RetrieveNumberOfConcurrentUsers(string ipAddress)
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            IConcurrentUsers concurrentUsers = mapper.ReadNumberOfConcurrentUsers(ipAddress);
            return concurrentUsers;
        }

        /// <summary>
        /// Retrieves the subscribers.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        public IEventSubscriber[] RetrieveSubscribers(string ipAddress)
        {
            List<IEventSubscriber> subscribers = new List<IEventSubscriber>();
            IEventSubscriber[] subscriberArray = RetrieveSubscribers();
            IList<IEventSubscriber> subscriberList = new List<IEventSubscriber>(subscriberArray);
            foreach (IEventSubscriber eventSubscriber in subscriberList)
            {
                if (eventSubscriber.IPAddress == ipAddress)
                    subscribers.Add(eventSubscriber);
            }
            return subscribers.ToArray();
        }

        /// <summary>
        /// Retrieves the addresses.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public IMessageInformation[] RetrieveAddresses()
        {
            return GetAddresses();
        }

        /// <summary>
        /// Gets the addresses.
        /// </summary>
        /// <returns></returns>
        private IMessageInformation[] GetAddresses()
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            return mapper.ReadAddressInfo();
        }

        /// <summary>
        /// Updates the configurations.
        /// </summary>
        /// <param name="configurations">The configurations.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public void UpdateConfigurations(IList<IConfigurationInfo> configurations)
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            mapper.UpdateConfigurations(configurations);
        }

        /// <summary>
        /// Deletes the configuration.
        /// </summary>
        /// <param name="configurationInfo">The configuration info.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void DeleteConfiguration(IConfigurationInfo configurationInfo)
        {
            ConfigurationDeleter deleter = new ConfigurationDeleter(ConnectionString);
            deleter.DeleteConfiguration(configurationInfo);
        }

        /// <summary>
        /// Updates the addresses.
        /// </summary>
        /// <param name="multicastAddressInfos">The multicast address infos.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        public void UpdateAddresses(IList<IMessageInformation> multicastAddressInfos)
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            IList<IAddressInformation> addressInfos = ConvertToAddressInfo(multicastAddressInfos);
            mapper.UpdateAddresses(addressInfos);
        }

        // ReSharper disable MemberCanBeMadeStatic
        /// <summary>
        /// Converts to address info.
        /// </summary>
        /// <param name="multicastAddressInfos">The multicast address infos.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private IList<IAddressInformation> ConvertToAddressInfo(IEnumerable<IMessageInformation> multicastAddressInfos)
        {
            IList<IAddressInformation> addressInfos = new List<IAddressInformation>();
            foreach (IMessageInformation messageInfo in multicastAddressInfos)
                addressInfos.Add(messageInfo);
            return addressInfos;
        }
        // ReSharper restore MemberCanBeMadeStatic

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void DeleteAddresses(IMessageInformation multicastAddressInfo)
        {
            AddressDeleter deleter = new AddressDeleter(ConnectionString);
            deleter.DeleteAddresses(multicastAddressInfo);
        }

        public IEventHeartbeat[] RetrieveHeartbeats()
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            return mapper.ReadHeartbeats();
        }


        public ILogbookEntry[] RetrieveLogbookEntries()
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            return mapper.ReadLogbookEntries();
        }


        public IConcurrentUsers RetrieveLogbookEntries(string ipAddress)
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            return mapper.ReadNumberOfConcurrentUsers(ipAddress);
        }

        /// <summary>
        /// Retrieves Event User Info
        /// </summary>
        /// <returns></returns>
        public IEventUser[] RetrieveEventUsers()
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            return mapper.ReadEventUsers();
        }

        /// <summary>
        /// Retrieve Event Receipt
        /// </summary>
        /// <returns></returns>
        public IEventReceipt[] RetrieveEventReceipt()
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            return mapper.ReadEventReceipt();
        }

        /// <summary>
        /// Retrieve Subscriber Information
        /// </summary>
        /// <returns></returns>
        public IEventSubscriber[] RetrieveSubscribers()
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            return mapper.ReadSubscribers();
        }

        /// <summary>
        /// Retrieve Filter Information
        /// </summary>
        /// <returns></returns>
        public IEventFilter[] RetrieveFilters()
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            return mapper.ReadFilters();
        }

        /// <summary>
        /// Retrieves the socket information.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        public ISocketInfo RetrieveSocketInformation()
        {
            IDataMapper mapper = new DataMapper(ConnectionString, RestartTime);
            return mapper.GetSocketInformation(ClientThrottle)[0];
        }

        /// <summary>
        /// Polls the specified subscriber id.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public IEventMessage[] Poll(Guid subscriberId)
        {
            return PollingManager.Instance.Poll(subscriberId);
        }

        #region IDisposable Override Implementation

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(true);
            if (isDisposing)
            {

            }
        }

        #endregion

    }
}