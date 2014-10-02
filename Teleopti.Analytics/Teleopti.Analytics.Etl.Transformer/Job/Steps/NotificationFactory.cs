using System;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client
{
	public class NotificationFactory
	{
		public static Notification CreateNotification(DateTime floor, DateTime ceiling, Guid moduleId, Guid domainObjectId,
		                                              Type domainInterfaceType, string dataSource, Guid businessUnitId)
		{
			return new Notification
				{
					StartDate = Subscription.DateToString(floor),
					EndDate = Subscription.DateToString(ceiling),
					DomainId = Subscription.IdToString(domainObjectId),
					DomainQualifiedType = domainInterfaceType.AssemblyQualifiedName,
					DomainType = domainInterfaceType.Name,
					ModuleId = Subscription.IdToString(moduleId),
					DomainUpdateType = (int)DomainUpdateType.Insert,
					DataSource = dataSource,
					BusinessUnitId = Subscription.IdToString(businessUnitId),
					BinaryData = null
				};
		}
	}
}