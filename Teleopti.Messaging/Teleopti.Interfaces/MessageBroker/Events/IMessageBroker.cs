using System;
using System.Diagnostics.CodeAnalysis;

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
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 16/04/2009
        /// </remarks>
        string ConnectionString { get; set; }
        
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
    }
}