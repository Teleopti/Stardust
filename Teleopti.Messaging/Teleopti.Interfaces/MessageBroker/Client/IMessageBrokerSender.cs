using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	public interface IMessageBrokerSender
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

		void Send(Notification notification);
	}
}