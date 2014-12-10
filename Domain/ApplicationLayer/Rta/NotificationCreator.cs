using System;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class NotificationCreator : INotificationCreator
	{
		public Notification Create(string datasource, Guid businessUnitId, string domainType)
		{
			return new Notification
			{
				DataSource = datasource,
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = domainType
			};
		}
	}
}