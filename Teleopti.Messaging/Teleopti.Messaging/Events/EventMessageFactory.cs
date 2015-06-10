using System;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Events
{
	public class EventMessageFactory : IEventMessageFactory
	{
		public IEventMessage CreateEventMessage(Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			return new EventMessage
			{
				ModuleId = moduleId,
				DomainObjectId = domainObjectId,
				DomainObjectType = domainObjectType.AssemblyQualifiedName,
				DomainUpdateType = updateType,
				EventStartDate = Consts.MinDate,
				EventEndDate = Consts.MaxDate
			};
		}

		public IEventMessage CreateEventMessage(Guid moduleId, Guid referenceObjectId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			return new EventMessage
			{
				ModuleId = moduleId,
				DomainObjectId = domainObjectId,
				DomainObjectType = domainObjectType.AssemblyQualifiedName,
				DomainUpdateType = updateType,
				ReferenceObjectId = referenceObjectId,
				EventStartDate = Consts.MinDate,
				EventEndDate = Consts.MaxDate
			};
		}

		public IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			return new EventMessage
			{
				ModuleId = moduleId,
				DomainObjectId = domainObjectId,
				DomainObjectType = domainObjectType.AssemblyQualifiedName,
				DomainUpdateType = updateType,
				EventStartDate = eventStartDate,
				EventEndDate = eventEndDate
			};
		}

		public IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			return new EventMessage
			{
				ModuleId = moduleId,
				DomainObjectId = domainObjectId,
				DomainObjectType = domainObjectType.AssemblyQualifiedName,
				DomainUpdateType = updateType,
				EventStartDate = eventStartDate,
				EventEndDate = eventEndDate,
				ReferenceObjectId = referenceObjectId,
			};
		}
	}
}