using System;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.MessageBroker.Client
{
	public interface IEventMessageFactory
	{
		IEventMessage CreateEventMessage(Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);
		IEventMessage CreateEventMessage(Guid moduleId, Guid referenceObjectId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);
		IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);
		IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);
	}
}