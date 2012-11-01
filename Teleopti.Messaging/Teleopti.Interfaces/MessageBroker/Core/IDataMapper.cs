using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The Broker's Data Mapper.
    /// </summary>
    public interface IDataMapper
    {
        /// <summary>
        /// Get Socket Information stored in the database.
        /// </summary>
        /// <returns></returns>
        IList<ISocketInfo> GetSocketInformation(int clientThrottle);

        /// <summary>
        /// Register a new user. The store procedure will check if the user is already registered.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Int32 RegisterUser(IEventUser user);

        /// <summary>
        /// Register a subscription to Events for the UserId and ProcessId in question.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        Guid RegisterSubscription(IEventSubscriber subscriber);

        /// <summary>
        /// Call this method to register a filter for which event messages should be propagated.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        IEventFilter RegisterFilter(IEventFilter filter);

        /// <summary>
        /// Deletes subscription from the database.
        /// </summary>
        /// <param name="subscriberId"></param>
        void UnregisterSubscription(Guid subscriberId);

        /// <summary>
        /// Deletes a filter from the database.
        /// </summary>
        /// <param name="filterId"></param>
        void UnregisterFilter(Guid filterId);

        /// <summary>
        /// Inserts an Event Message without payload.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Guid InsertEventMessage(IEventMessage message);

        /// <summary>
        /// Inserts an Event Heartbeat
        /// </summary>
        /// <param name="beat"></param>
        /// <returns></returns>
        Guid InsertEventHeartbeat(IEventHeartbeat beat);

        /// <summary>
        /// Inserts an Event Receipt
        /// </summary>
        /// <param name="receipt"></param>
        /// <returns></returns>
        Guid InsertEventReceipt(IEventReceipt receipt);

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
        Guid InsertEventMessage(DateTime eventStartDate,
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
                                string raisedBy);

        /// <summary>
        /// Inserts the configuration info.
        /// </summary>
        /// <param name="configurationType">Type of the configuration.</param>
        /// <param name="configurationName">Name of the configuration.</param>
        /// <param name="configurationValue">The configuration value.</param>
        /// <param name="configurationDataType">Type of the configuration data.</param>
        /// <param name="changedBy">The changed by.</param>
        /// <returns></returns>
        Int32 InsertConfigurationInfo(string configurationType, string configurationName, string configurationValue, string configurationDataType, string changedBy);
        /// <summary>
        /// Reads the configuration info.
        /// </summary>
        /// <param name="configurationType">Type of the configuration.</param>
        /// <returns></returns>
        IConfigurationInfo[] ReadConfigurationInfo(string configurationType);
        /// <summary>
        /// Runs the scavenge.
        /// </summary>
        void RunScavenge();
        /// <summary>
        /// Updates the configurations.
        /// </summary>
        /// <param name="configurations">The configurations.</param>
        void UpdateConfigurations(IList<IConfigurationInfo> configurations);
        /// <summary>
        /// Updates the addresses.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        void UpdateAddresses(IList<IAddressInformation> addresses);
        /// <summary>
        /// Reads the address info.
        /// </summary>
        /// <returns></returns>
        IMessageInformation[] ReadAddressInfo();
        /// <summary>
        /// Reads the heartbeats.
        /// </summary>
        /// <returns></returns>
        IEventHeartbeat[] ReadHeartbeats();
        /// <summary>
        /// Reads the number of concurrent users.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        IConcurrentUsers ReadNumberOfConcurrentUsers(string ipAddress);
        /// <summary>
        /// Reads the event users.
        /// </summary>
        /// <returns></returns>
        IEventUser[] ReadEventUsers();
        /// <summary>
        /// Reads the event receipt.
        /// </summary>
        /// <returns></returns>
        IEventReceipt[] ReadEventReceipt();
        /// <summary>
        /// Reads the subscribers.
        /// </summary>
        /// <returns></returns>
        IEventSubscriber[] ReadSubscribers();
        /// <summary>
        /// Reads the filters.
        /// </summary>
        /// <returns></returns>
        IEventFilter[] ReadFilters();

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
        void UpdatePortForSubscriber(Guid subscriberId, int port);

        /// <summary>
        /// Deleteds the heartbeat.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        void DeletedHeartbeat();

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
        void DeletedReceipts();

        /// <summary>
        /// Checks the subscription status.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        bool CheckSubscriptionStatus(Guid subscriberId);

        /// <summary>
        /// Retrieves the distinct heartbeats.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        IDictionary<Guid, IEventHeartbeat> RetrieveDistinctHeartbeats();
    }
}