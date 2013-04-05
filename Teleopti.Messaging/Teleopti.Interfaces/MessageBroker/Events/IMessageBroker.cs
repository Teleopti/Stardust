using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	/// <summary>
    /// The client MessageBroker instance will enable
    /// ASM and RAPTOR clients to publish and subscribe
    /// to messages.
    /// </summary>
    public interface IMessageBroker : IDisposable, IMessageBrokerSender, IMessageBrokerListener
	{
		/// <summary>
        /// Creates event message, ignoring dates and payload.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="domainObjectId"></param>
        /// <param name="domainObjectType"></param>
        /// <param name="updateType"></param>
        /// <returns></returns>
        IEventMessage CreateEventMessage(Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);

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
        IEventMessage CreateEventMessage(Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);

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
        IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);

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
        IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);

        /// <summary>
        /// The configuration settings of this instance of the message broker, which is received from the server.
        /// </summary>
        /// <returns></returns>
        IList<IConfigurationInfo> RetrieveConfigurations();

        /// <summary>
        /// The Addresses
        /// </summary>
        /// <returns></returns>
        IList<IMessageInformation> RetrieveAddresses();

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <param name="configurations"></param>
        void UpdateConfigurations(IList<IConfigurationInfo> configurations);

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <param name="configurationInfo"></param>
        void DeleteConfigurationItem(IConfigurationInfo configurationInfo);

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <param name="addresses"></param>
        void UpdateAddresses(IList<IMessageInformation> addresses);

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <param name="addressInfo"></param>
        void DeleteAddressItem(IMessageInformation addressInfo);

        /// <summary>
        /// Used by Management Client, can be ignored by Raptor Developer.
        /// </summary>
        /// <returns></returns>
        IEventHeartbeat[] RetrieveHeartbeats();

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
        /// throws BrokerNotInstantiatedException if it fails.
        /// Please catch this exception and propagate to user
        /// since it can be a valid exception.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void StartMessageBroker();

        /// <summary>
        /// Stops the message broker.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void StopMessageBroker();

        /// <summary>
        /// Gets or sets the initialized.
        /// </summary>
        /// <value>The initialized.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        int Initialized { get; set; }
        /// <summary>
        /// Gets or sets the remoting port.
        /// </summary>
        /// <value>The remoting port.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        int RemotingPort { get; set; }
        /// <summary>
        /// Gets or sets the threads.
        /// </summary>
        /// <value>The threads.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        int Threads { get; set; }
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        int UserId { get; set; }
        /// <summary>
        /// Gets or sets the messaging protocol.
        /// </summary>
        /// <value>The messaging protocol.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        MessagingProtocol MessagingProtocol { get; set; }
        /// <summary>
        /// Gets or sets the messaging port.
        /// </summary>
        /// <value>The messaging port.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        int MessagingPort { get; set; }
        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>The server.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        string Server { get; set; }
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        string ConnectionString { get; set; }
        /// <summary>
        /// Gets or sets the subscriber id.
        /// </summary>
        /// <value>The subscriber id.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        Guid SubscriberId { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is type filter applied.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is type filter applied; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        bool IsTypeFilterApplied { get; set; }
        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        bool IsInitialized { get; }

        /// <summary>
        /// Occurs when [event message handler].
        /// </summary>
        event EventHandler<EventMessageArgs> EventMessageHandler;

        /// <summary>
        /// Occurs when [exception handler].
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> ExceptionHandler;

        /// <summary>
        /// If you need to restart the message broker.
        /// </summary>
        /// <returns></returns>
        bool Restart();

        /// <summary>
        /// Sends the receipt.
        /// </summary>
        /// <param name="message">The message.</param>
        void SendReceipt(IEventMessage message);

        /// <summary>
        /// Creates the addresses.
        /// </summary>
        /// <returns></returns>
        IMessageInformation[] CreateAddresses();

        /// <summary>
        /// Creates the configurations.
        /// </summary>
        /// <returns></returns>
        IConfigurationInfo[] CreateConfigurations();

        /// <summary>
        /// Internals the log.
        /// </summary>
        /// <param name="exception">The exception.</param>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        void InternalLog(Exception exception);

        /// <summary>
        /// Services the guard.
        /// </summary>
        /// <param name="service">The service.</param>
        void ServiceGuard(IBrokerService service);

    }
}