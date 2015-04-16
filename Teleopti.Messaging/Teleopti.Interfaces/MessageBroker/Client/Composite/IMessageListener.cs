using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Client.Composite
{
	public interface IMessageListener
	{
		void RegisterSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType);
		void RegisterSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType);
		void RegisterSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate);
		void RegisterSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate);
		void RegisterSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate);
		void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler);
	}
}