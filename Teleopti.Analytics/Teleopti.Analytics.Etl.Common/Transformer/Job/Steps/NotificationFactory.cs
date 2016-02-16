using System;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Messaging.Client
{
	public class NotificationFactory
	{
		public static Message CreateNotification(DateTime floor, DateTime ceiling, Guid moduleId, Guid domainObjectId,
		                                              Type domainInterfaceType, string dataSource, Guid businessUnitId)
		{
			return new Message
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