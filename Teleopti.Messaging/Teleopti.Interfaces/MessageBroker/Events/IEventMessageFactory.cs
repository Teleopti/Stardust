using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	public interface IEventMessageFactory
	{
		IEventMessage CreateEventMessage(Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);
		IEventMessage CreateEventMessage(Guid moduleId, Guid referenceObjectId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);
		IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);
		IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);
	}
}