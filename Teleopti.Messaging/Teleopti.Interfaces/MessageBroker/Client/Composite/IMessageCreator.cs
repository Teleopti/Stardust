using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Client.Composite
{
	public interface IMessageCreator
	{
		void Send(
			string dataSource,
			Guid businessUnitId,
			DateTime eventStartDate,
			DateTime eventEndDate,
			Guid moduleId,
			Guid referenceObjectId,
			Type referenceObjectType,
			Guid domainObjectId,
			Type domainObjectType,
			DomainUpdateType updateType,
			byte[] domainObject);

		void Send(
			string dataSource,
			Guid businessUnitId,
			DateTime eventStartDate,
			DateTime eventEndDate,
			Guid moduleId,
			Guid domainObjectId,
			Type domainObjectType,
			DomainUpdateType updateType,
			byte[] domainObject);

		void Send(
			string dataSource,
			Guid businessUnitId,
			IEventMessage[] eventMessages);
	}
}