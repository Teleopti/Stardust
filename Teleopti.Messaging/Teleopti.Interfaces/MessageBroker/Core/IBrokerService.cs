using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The Broker Service.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IBrokerService
    {
        /// <summary>
        /// Initialises the specified publisher.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        /// <param name="connectionString">The connection string.</param>
        void Initialize(IPublisher publisher, string connectionString);

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
        Int32 RegisterUser(string domain, string userName);

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
        Guid RegisterSubscriber(Int32 userId, string userName, Int32 processId, string ipAddress, int port);

        /// <summary>
        /// Unregisters the subscriber.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        void UnregisterSubscriber(string address, int port);

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
        void UpdatePortForSubscriber(Guid subscriberId, int port);

        /// <summary>
        /// Unregisters the subscriber.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        void UnregisterSubscriber(Guid subscriberId);

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
        IEventFilter RegisterFilter(Guid subscriberId, 
                                    Guid domainObjectId, 
                                    string domainObjectType, 
                                    DateTime startDate, 
                                    DateTime endDate, 
                                    string userName);

        /// <summary>
        /// Registers the filter.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="referenceObjectId">The parent object id.</param>
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
        IEventFilter RegisterFilter(Guid subscriberId, 
                                    Guid referenceObjectId,
                                    string referenceObjectType, 
                                    Guid domainObjectId, 
                                    string domainObjectType, 
                                    DateTime startDate, 
                                    DateTime endDate, 
                                    string userName);

        /// <summary>
        /// Unregisters the filter.
        /// </summary>
        /// <param name="filterId">The filter id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        void UnregisterFilter(Guid filterId);

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
        /// Created date: 2009-03-29
        /// </remarks>
        void SendEventMessage(DateTime eventStartDate,
                             DateTime eventEndDate,
                             Int32 userId,
                             Int32 processId,
                             Guid moduleId,
                             Int32 packageSize,
                             bool isHeartbeat,
                             Guid domainObjectId,
                             string domainObjectType,
                             DomainUpdateType updateType,
                             string userName);

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
        void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, Int32 userId, Int32 processId, Guid moduleId, Int32 packageSize, bool isHeartbeat, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, byte[] domainObject, string userName);

        /// <summary>
        /// Sends the event messages.
        /// </summary>
        /// <param name="eventMessages">The event messages.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        void SendEventMessages(IEventMessage[] eventMessages);

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
        void Log(Int32 processId, string description, string exception, string message, string stackTrace, string userName);

        /// <summary>
        /// Retrieves the configurations.
        /// </summary>
        /// <param name="configurationType">Type of the configuration.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        IConfigurationInfo[] RetrieveConfigurations(string configurationType);

        /// <summary>
        /// Sends the receipt.
        /// </summary>
        /// <param name="receipt">The receipt.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        void SendReceipt(IEventReceipt receipt);

        /// <summary>
        /// Sends the heartbeat.
        /// </summary>
        /// <param name="beat">The beat.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        void SendHeartbeat(IEventHeartbeat beat);

        /// <summary>
        /// Gets the threads.
        /// </summary>
        /// <value>The threads.</value>
        int Threads { get; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        string ConnectionString { get; }

        /// <summary>
        /// Gets or sets the event subscriptions.
        /// </summary>
        /// <value>The event subscriptions.</value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<IEventSubscriber> EventSubscriptions { get; set; }

        /// <summary>
        /// Gets the protocol.
        /// </summary>
        /// <value>The protocol.</value>
        MessagingProtocol Protocol { get; }

        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>The filters.</value>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<Guid, IList<IEventFilter>> Filters { get; }

        /// <summary>
        /// Gets the client throttle.
        /// </summary>
        /// <value>The client throttle.</value>
        int ClientThrottle { get; }

        /// <summary>
        /// Gets or sets the messaging port.
        /// </summary>
        /// <value>The messaging port.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        int MessagingPort { get; set; }

        /// <summary>
        /// Retrieves the number of concurrent users.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        IConcurrentUsers RetrieveNumberOfConcurrentUsers(string ipAddress);

        /// <summary>
        /// Retrieves the subscribers.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        IEventSubscriber[] RetrieveSubscribers(string ipAddress);

        /// <summary>
        /// Retrieves the addresses.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        IMessageInformation[] RetrieveAddresses();

        /// <summary>
        /// Updates the configurations.
        /// </summary>
        /// <param name="configurations">The configurations.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        void UpdateConfigurations(IList<IConfigurationInfo> configurations);

        /// <summary>
        /// Deletes the configuration.
        /// </summary>
        /// <param name="configurationInfo">The configuration info.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void DeleteConfiguration(IConfigurationInfo configurationInfo);

        /// <summary>
        /// Updates the addresses.
        /// </summary>
        /// <param name="multicastAddressInfos">The multicast address infos.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Infos")]
        void UpdateAddresses(IList<IMessageInformation> multicastAddressInfos);

        /// <summary>
        /// Deletes the addresses.
        /// </summary>
        /// <param name="multicastAddressInfo">The multicast address info.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void DeleteAddresses(IMessageInformation multicastAddressInfo);

        /// <summary>
        /// Retrieves the heartbeats.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        IEventHeartbeat[] RetrieveHeartbeats();
        /// <summary>
        /// Retrieves the logbook entries.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        ILogbookEntry[] RetrieveLogbookEntries();

        /// <summary>
        /// Retrieves Event User Info
        /// </summary>
        /// <returns></returns>
        IEventUser[] RetrieveEventUsers();

        /// <summary>
        /// Retrieve Event Receipt
        /// </summary>
        /// <returns></returns>
        IEventReceipt[] RetrieveEventReceipt();

        /// <summary>
        /// Retrieve Subscriber Information
        /// </summary>
        /// <returns></returns>
        IEventSubscriber[] RetrieveSubscribers();

        /// <summary>
        /// Retrieve Filter Information
        /// </summary>
        /// <returns></returns>
        IEventFilter[] RetrieveFilters();

        /// <summary>
        /// Retrieves the socket information.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        ISocketInfo RetrieveSocketInformation();

        /// <summary>
        /// Polls the specified subscriber id.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        IEventMessage[] Poll(Guid subscriberId);

    }
}