using System;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.SignalR
{
	public class DoNotSend : IMessageBrokerSender
	{
		public void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
		}

		public void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
		}

		public void SendEventMessages(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
		}

		public void SendNotification(Notification notification)
		{
		}
	}
}