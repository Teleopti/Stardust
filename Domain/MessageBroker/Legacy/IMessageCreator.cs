using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.MessageBroker.Legacy
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
			byte[] domainObject,
			Guid? trackId = null);

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