﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Logging.Core;
using Teleopti.Messaging.DataAccessLayer;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Core
{

    /// <summary>
    /// The Broker Processor.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2009-03-29
    /// </remarks>
    public class BrokerProcessor : IBrokerProcessor
    {
        private static readonly object _scavageLock = new object();
        private readonly string _connectionString;
        private readonly long _restartTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokerProcessor"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="restartTime">The restart time.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        public BrokerProcessor(string connectionString, long restartTime)
        {
            _connectionString = connectionString;
            _restartTime = restartTime;
        }

        /// <summary>
        /// Create a new user. 
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IEventUser CreateUser(string domain, string userName)
        {
            IEventUser newUser = new EventUser(0, domain, userName, userName, DateTime.Now);
            return newUser;
        }

        /// <summary>
        /// Create a Heartbeat to signal messaging is still working.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public IEventHeartbeat CreateHeartbeat(Guid subscriberId, Int32 processId, string userName)
        {
            IEventHeartbeat beat = new EventHeartbeat(Guid.NewGuid(), subscriberId, processId, userName, DateTime.Now);
            return beat;
        }

        /// <summary>
        /// Create a Receipt to signal messaging is still working.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="processId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IEventReceipt CreateReceipt(Guid eventId, Int32 processId, string userName)
        {
            IEventReceipt receipt = new EventReceipt(Guid.NewGuid(), eventId, processId, userName, DateTime.Now);
            return receipt;
        }

        /// <summary>
        /// Create a subscription to Events for the UserId and ProcessId in question.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="processId"></param>
        /// <param name="userName"></param>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public IEventSubscriber CreateSubscription(Int32 userId, Int32 processId, string userName, string ipAddress, int port)
        {
            IEventSubscriber subscriber = new EventSubscriber(Guid.NewGuid(), userId, processId, ipAddress, port, userName, DateTime.Now);
            return subscriber;
        }

        /// <summary>
        /// Call this method to create a filter for which event messages should be propagated.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public IEventFilter CreateFilter(Guid subscriberId, Guid parentObjectId, string parentObjectType, Guid domainObjectId, string domainObjectType, DateTime startDate, DateTime endDate, string userName)
        {
            IEventFilter filter = new EventFilter(Guid.NewGuid(), subscriberId, parentObjectId, parentObjectType, domainObjectId, domainObjectType, startDate, endDate, userName, DateTime.Now);
            return filter;
        }

        /// <summary>
        /// Create new Event Message.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="packageSize">Size of the package.</param>
        /// <param name="isHeartbeat">if set to <c>true</c> [is heartbeat].</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Int32 userId, Int32 processId, Guid moduleId, Int32 packageSize, bool isHeartbeat, Guid parentObjectId, string parentObjectType, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, string userName)
        {
            IEventMessage message = new EventMessage(Guid.NewGuid(), eventStartDate, eventEndDate, userId, processId, moduleId, packageSize, isHeartbeat, parentObjectId, parentObjectType, domainObjectId, domainObjectType, updateType, userName, DateTime.Now);
            return message;
        }

        /// <summary>
        /// Create new Event Message.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="packageSize">Size of the package.</param>
        /// <param name="isHeartbeat">if set to <c>true</c> [is heartbeat].</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <param name="domainObject">The domain object.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Int32 userId, Int32 processId, Guid moduleId, Int32 packageSize, bool isHeartbeat, Guid parentObjectId, string parentObjectType, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, byte[] domainObject, string userName)
        {
            IEventMessage message = new EventMessage(Guid.NewGuid(), eventStartDate, eventEndDate, userId, processId, moduleId, packageSize, isHeartbeat, parentObjectId, parentObjectType, domainObjectId, domainObjectType, updateType, domainObject, userName, DateTime.Now);
            return message;
        }

        /// <summary>
        /// Creates a new Event Log Entry.
        /// </summary>
        /// <param name="processId">The process id.</param>
        /// <param name="description">The description.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="stackTrace">The stack trace.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public ILogEntry CreateEventLogEntry(Int32 processId, string description, string exception, string message, string stackTrace, string userName)
        {
            ILogEntry eventLogEntry = new LogEntry(Guid.NewGuid(), processId, description, exception, message, stackTrace, userName, DateTime.Now);
            return eventLogEntry;
        }


        /// <summary>
        /// Get Socket Information stored in the database.
        /// </summary>
        /// <returns></returns>
        public IList<ISocketInfo> GetSocketInformation(int clientThrottle)
        {
            IList<ISocketInfo> socketInfos = new List<ISocketInfo>();
            try
            {
                AddressReader reader = new AddressReader(_connectionString);
                IList<IAddressInfo> addressInfos = reader.Execute();
                foreach (IAddressInfo addressInfo in addressInfos)
                {
                    socketInfos.Add(new SocketInfo(addressInfo.Address, addressInfo.Port, clientThrottle));
                }
            }
            catch (Exception exception)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "GetSocketInformation(int clientThrottle) failed. {0}.", exception));
            }
            return socketInfos;
        }

        /// <summary>
        /// Register a new user. The store procedure will check if the user is already registered.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Int32 RegisterUser(IEventUser user)
        {
            try
            {
                EventUserInserter inserter = new EventUserInserter(_connectionString);
                inserter.Execute(user);
                return user.UserId;
            }
            catch (Exception exception)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "RegisterUser(IEventUser user) failed. {0}.", exception));
                return -1;
            }
        }

        /// <summary>
        /// Register a subscription to Events for the UserId and ProcessId in question.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public Guid RegisterSubscription(IEventSubscriber subscriber)
        {
            try
            {
                EventSubscriberInserter inserter = new EventSubscriberInserter(_connectionString);
                inserter.Execute(subscriber);
                return subscriber.SubscriberId;
            }
            catch (Exception exception)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "RegisterSubscription(IEventSubscriber subscriber) failed. {0}.", exception));
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Call this method to register a filter for which event messages should be propagated.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEventFilter RegisterFilter(IEventFilter filter)
        {
            EventFilterInserter inserter = new EventFilterInserter(_connectionString);
            inserter.Execute(filter);
            return filter;
        }

        /// <summary>
        /// Deletes subscription from the database.
        /// </summary>
        /// <param name="subscriberId"></param>
        public void UnregisterSubscription(Guid subscriberId)
        {
            FilterDeleter deleter = new FilterDeleter(_connectionString);
            deleter.UnregisterFilter(subscriberId);
        }

        /// <summary>
        /// Deletes a filter from the database.
        /// </summary>
        /// <param name="filterId"></param>
        public void UnregisterFilter(Guid filterId)
        {
            SubscriberDeleter deleter = new SubscriberDeleter(_connectionString);
            deleter.UnregisterSubscription(filterId);
        }

        /// <summary>
        /// Inserts an Event Message without payload.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Guid InsertEventMessage(IEventMessage message)
        {
            EventMessageInserter inserter = new EventMessageInserter(_connectionString);
            inserter.Execute(message);
            return message.EventId;
        }

        /// <summary>
        /// Inserts an Event Heartbeat
        /// </summary>
        /// <param name="beat"></param>
        /// <returns></returns>
        public Guid InsertEventHeartbeat(IEventHeartbeat beat)
        {
            HeartbeatInserter inserter = new HeartbeatInserter(_connectionString);
            inserter.Execute(beat);
            return beat.HeartbeatId;
        }

        /// <summary>
        /// Inserts an Event Receipt
        /// </summary>
        /// <param name="receipt"></param>
        /// <returns></returns>
        public Guid InsertEventReceipt(IEventReceipt receipt)
        {
            ReceiptInserter inserter = new ReceiptInserter(_connectionString);
            inserter.Execute(receipt);
            return receipt.ReceiptId;
        }

        /// <summary>
        /// Inserts an Event Message with payload.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="packageSize">Size of the package.</param>
        /// <param name="isHeartbeat">if set to <c>true</c> [is heartbeat].</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="domainUpdateType">Type of the domain update.</param>
        /// <param name="domainObject">The domain object.</param>
        /// <param name="raisedBy">The raised by.</param>
        /// <returns></returns>
        public Guid InsertEventMessage(DateTime eventStartDate,
                                        DateTime eventEndDate,
                                        Int32 userId,
                                        Int32 processId,
                                        Guid moduleId,
                                        Int32 packageSize,
                                        bool isHeartbeat,
                                        Guid parentObjectId,
                                        string parentObjectType,
                                        Guid domainObjectId,
                                        string domainObjectType,
                                        DomainUpdateType domainUpdateType,
                                        byte[] domainObject,
                                        string raisedBy)
        {
            IEventMessage message = new EventMessage(Guid.Empty, eventStartDate, eventEndDate, userId, processId, moduleId, packageSize, isHeartbeat, parentObjectId, parentObjectType, domainObjectId, domainObjectType, domainUpdateType, domainObject, raisedBy, DateTime.Now);
            EventMessageInserter inserter = new EventMessageInserter(_connectionString);
            inserter.Execute(message);
            return message.EventId;
        }

        /// <summary>
        /// Inserts a log entry into the database on whether the event was propagated successfully or not.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="description"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="changeBy"></param>
        /// <returns></returns>
        public Guid InsertEventLogEntry(Int32 processId, string description, string exception, string message, string stackTrace, string changeBy)
        {
            ILogEntry logEntry = new LogEntry(Guid.Empty, processId, description, exception, message, stackTrace, changeBy, DateTime.Now);
            LogEntryInserter inserter = new LogEntryInserter(_connectionString);
            inserter.Execute(logEntry);
            return logEntry.LogId;
        }

        public Int32 InsertConfigurationInfo(string configurationType, string configurationName, string configurationValue, string configurationDataType, string changedBy)
        {
            IConfigurationInfo configurationInfo = new ConfigurationInfo(0,
                                                                         configurationType,
                                                                         configurationName,
                                                                         configurationValue,
                                                                         configurationDataType,
                                                                         changedBy,
                                                                         DateTime.Now);
            ConfigurationInfoInserter inserter = new ConfigurationInfoInserter(_connectionString);
            inserter.Execute(configurationInfo);
            return configurationInfo.ConfigurationId;
        }

        public IConfigurationInfo[] ReadConfigurationInfo(string configurationType)
        {
            ConfigurationInfoReader reader = new ConfigurationInfoReader(_connectionString);
            List<IConfigurationInfo> configurationInfos = (List<IConfigurationInfo>)reader.Execute();
            List<IConfigurationInfo> configurationInfoByType = new List<IConfigurationInfo>();
            if (String.IsNullOrEmpty(configurationType))
            {
                return configurationInfos.ToArray();
            }
            foreach (IConfigurationInfo configurationInfo in configurationInfos)
            {
                if (configurationInfo.ConfigurationType == configurationType)
                    configurationInfoByType.Add(configurationInfo);
            }
            return configurationInfoByType.ToArray();
        }

        public void RunScavage()
        {
            RunScavageEvents();
            TimeSpan restartTimeSpan = new TimeSpan(0, 0, 0, 0, (int)_restartTime);
            RunScavageSubscriber(DateTime.Now.Subtract(restartTimeSpan));
        }

        public void UpdateConfigurations(IList<IConfigurationInfo> configurations)
        {
            ConfigurationInfoInserter inserter = new ConfigurationInfoInserter(_connectionString);
            inserter.Execute(configurations);
        }

        #pragma warning disable 1692

        public void UpdateAddresses(IList<IAddressInfo> addresses)
        {
            AddressInserter inserter = new AddressInserter(_connectionString);
            inserter.Execute(addresses);
        }

        public IMessageInfo[] ReadAddressInfo()
        {
            AddressReader reader = new AddressReader(_connectionString);
            List<IMessageInfo> addresses = (List<IMessageInfo>)reader.Execute();
            return addresses.ToArray();
        }

        public IEventHeartbeat[] ReadHeartbeats()
        {
            HeartbeatReader reader = new HeartbeatReader(_connectionString);
            List<IEventHeartbeat> heartbeats = (List<IEventHeartbeat>)reader.Execute();
            return heartbeats.ToArray();
        }

        public IConcurrentUsers ReadNumberOfConcurrentUsers(string ipAddress)
        {
            ConcurrentUsersReader reader = new ConcurrentUsersReader(_connectionString);
            IConcurrentUsers numberOfUsers = reader.Execute(ipAddress);
            return numberOfUsers;
        }

        public IEventUser[] ReadEventUsers()
        {
            EventUserReader reader = new EventUserReader(_connectionString);
            List<IEventUser> eventUsers = (List<IEventUser>)reader.Execute();
            return eventUsers.ToArray();
        }

        public IEventReceipt[] ReadEventReceipt()
        {
            ReceiptReader reader = new ReceiptReader(_connectionString);
            List<IEventReceipt> eventReceipt = (List<IEventReceipt>)reader.Execute();
            return eventReceipt.ToArray();
        }

        public IEventSubscriber[] ReadSubscribers()
        {
            EventSubscriberReader reader = new EventSubscriberReader(_connectionString);
            List<IEventSubscriber> eventSubscriber = (List<IEventSubscriber>)reader.Execute();
            return eventSubscriber.ToArray();
        }

        public IEventFilter[] ReadFilters()
        {
            EventFilterReader reader = new EventFilterReader(_connectionString);
            List<IEventFilter> eventFilter = (List<IEventFilter>)reader.Execute();
            return eventFilter.ToArray();
        }

        public ILogbookEntry[] ReadLogbookEntries()
        {
            LogEntryReader reader = new LogEntryReader(_connectionString);
            IList<ILogEntry> logEntries = reader.Execute();
            return CreateLogbookEntries(logEntries);
        }

        // ReSharper disable MemberCanBeMadeStatic
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private ILogbookEntry[] CreateLogbookEntries(IEnumerable<ILogEntry> entries)
        {
            List<ILogbookEntry> logBookEntries = new List<ILogbookEntry>();
            foreach (ILogEntry entry in entries)
            {
                logBookEntries.Add(new LogbookEntry(entry));
            }
            return logBookEntries.ToArray();
        }
        // ReSharper restore MemberCanBeMadeStatic

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void RunScavageEvents()
        {
            lock (_scavageLock)
            {
                EventDeleter deleter = new EventDeleter(_connectionString);
                deleter.DeleteEvent();
            }
        }

        /// <summary>
        /// Scavage subscribers.
        /// </summary>
        /// <param name="scavageDateTimeWindow">The scavage date time window.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void RunScavageSubscriber(DateTime scavageDateTimeWindow)
        {
            lock (_scavageLock)
            {
                SubscriberScavager scavager = new SubscriberScavager(_connectionString);
                scavager.ScavageSubscriber(scavageDateTimeWindow);
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
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void UpdatePortForSubscriber(Guid subscriberId, int port)
        {
            try
            {
                EventSubscriberReader reader = new EventSubscriberReader(_connectionString);
                IEventSubscriber eventSubscriber = reader.Execute(subscriberId);
                eventSubscriber.Port = port;
                EventSubscriberInserter inserter = new EventSubscriberInserter(_connectionString);
                inserter.Execute(eventSubscriber);
            }
            catch (Exception exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.Message + exc.StackTrace);
            }
        }

        /// <summary>
        /// Deleteds the heartbeat.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        public void DeletedHeartbeat()
        {
            HeartbeatDeleter deleter = new HeartbeatDeleter(_connectionString);
            deleter.DeleteHeartbeats();
        }

        /// <summary>
        /// Deleteds the receipts.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        public void DeletedReceipts()
        {
            ReceiptDeleter deleter = new ReceiptDeleter(_connectionString);
            deleter.DeleteReceipts();
        }

        /// <summary>
        /// Checks the subscription status.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        public bool CheckSubscriptionStatus(Guid subscriberId)
        {
            HeartbeatReader reader = new HeartbeatReader(_connectionString);
            IList<IEventHeartbeat> heartbeats = reader.GetAllHeartbeatsForSubscriber(subscriberId);
            return heartbeats.Count > 0;
        }

        /// <summary>
        /// Retrieves the distinct heartbeats.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        public IDictionary<Guid, IEventHeartbeat> RetrieveDistinctHeartbeats()
        {
            HeartbeatReader reader = new HeartbeatReader(_connectionString);
            IList<IEventHeartbeat> heartbeats = reader.DistinctHeartbeats();
            IDictionary<Guid, IEventHeartbeat> heartbeatDictionary = new Dictionary<Guid, IEventHeartbeat>();
            foreach (IEventHeartbeat heartbeat in heartbeats)
            {
                heartbeatDictionary.Add(heartbeat.SubscriberId, heartbeat);
            }
            return heartbeatDictionary;
        }

#pragma warning restore 1692

    }
}