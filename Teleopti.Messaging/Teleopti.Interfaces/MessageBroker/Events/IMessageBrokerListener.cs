using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	/// <summary>
	/// The client MessageBroker instance will enable
	/// ASM and RAPTOR clients to subscribe
	/// to messages.
	/// </summary>
	public interface IMessageBrokerListener
	{
		/// <summary>
		/// Registers the event subscription.
		/// Designated method for Raptor Developers to Register Event Subscriptions
		/// passing in a delegate where no filters are applicable.
		/// </summary>
		/// <param name="eventMessageHandler">The event message handler.</param>
		/// <param name="domainObjectType">Type of the domain object.</param>
		/// <param name="businessUnitId"></param>
		/// <param name="dataSource"></param>
		/// <remarks>
		/// Created by: ankarlp
		/// </remarks>
		void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType);

		/// <summary>
		/// Registers the event subscription.
		/// </summary>
		/// <param name="eventMessageHandler">The event message handler.</param>
		/// <param name="referenceObjectId">The reference object id.</param>
		/// <param name="referenceObjectType">Type of the reference object.</param>
		/// <param name="domainObjectType">Type of the domain object.</param>
		/// <param name="businessUnitId"></param>
		/// <param name="dataSource"></param>
		/// <remarks>
		/// Created by: ankarlp
		/// Created date: 16/04/2009
		/// </remarks>
		void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType);

		/// <summary>
		/// Method for Raptor Developers to Register Event Subscriptions for the Scheduler,
		/// a delegate and filter criterias including dates is taken as arguments but no Guid.
		/// </summary>
		/// <param name="eventMessageHandler"></param>
		/// <param name="domainObjectType"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="businessUnitId"></param>
		/// <param name="dataSource"></param>
		void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate);

		/// <summary>
		/// Method for Raptor Developers to Register Event Subscriptions,
		/// a delegate and filter criterias including dates is taken as arguments.
		/// </summary>
		/// <param name="eventMessageHandler"></param>
		/// <param name="domainObjectId"></param>
		/// <param name="domainObjectType"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="businessUnitId"></param>
		/// <param name="dataSource"></param>
		void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate);

		/// <summary>
		/// Unregister a delegate and all filters associated will be unregistered as well.
		/// </summary>
		/// <param name="eventMessageHandler"></param>
		void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler);

		/// <summary>
		/// Method for Raptor Developers to Register Event Subscriptions,
		/// a delegate and filter criterias including dates is taken as arguments.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="businessUnitId"></param>
		/// <param name="eventMessageHandler"></param>
		/// <param name="referenceObjectId"></param>
		/// <param name="referenceObjectType"></param>
		/// <param name="domainObjectType"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate);
	}
}