using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class NotificationCreator : INotificationCreator
	{
		public Interfaces.MessageBroker.Notification Create(string datasource, Guid businessUnitId, string domainType)
		{
			return new Interfaces.MessageBroker.Notification
			{
				DataSource = datasource,
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = domainType
			};
		}
	}
}