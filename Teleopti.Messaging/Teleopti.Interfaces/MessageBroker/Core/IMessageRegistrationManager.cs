using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The Message Registration Manager
    /// </summary>
    public interface IMessageRegistrationManager
    {
        /// <summary>
        /// Registers the event subscription.
        /// </summary>
        /// <param name="eventMessageHandler">The event message handler.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Type parentObjectType, Type domainObjectType);
        /// <summary>
        /// Registers the event subscription.
        /// </summary>
        /// <param name="eventMessageHandler">The event message handler.</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType);
        /// <summary>
        /// Registers the event subscription.
        /// </summary>
        /// <param name="eventMessageHandler">The event message handler.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Type parentObjectType, Type domainObjectType, DateTime startDate, DateTime endDate);
        /// <summary>
        /// Registers the event subscription.
        /// </summary>
        /// <param name="eventMessageHandler">The event message handler.</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate);
        /// <summary>
        /// Unregisters the event subscription.
        /// </summary>
        /// <param name="eventMessageHandler">The event message handler.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler);
        /// <summary>
        /// Checks the filters.
        /// </summary>
        /// <param name="eventMessageArgs">The event message args.</param>
        void CheckFilters(EventMessageArgs eventMessageArgs);
        /// <summary>
        /// Registers the filter.
        /// </summary>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        IEventFilter RegisterFilter(Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate);
        /// <summary>
        /// Unregisters the filter.
        /// </summary>
        /// <param name="filterId">The filter id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        void UnregisterFilter(Guid filterId);
        
        /// <summary>
        /// Reintializes the filters.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        void ReinitializeFilters();

        /// <summary>
        /// Unregisters the filters.
        /// </summary>
        void UnregisterFilters();

        /// <summary>
        /// Gets or sets the broker service.
        /// </summary>
        /// <value>The broker service.</value>
        IBrokerService BrokerService { get; set; }
    }
}