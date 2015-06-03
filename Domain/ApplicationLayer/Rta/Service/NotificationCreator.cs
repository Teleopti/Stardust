using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class NotificationCreator : INotificationCreator
	{
		public Interfaces.MessageBroker.Message Create(string datasource, Guid businessUnitId, string domainType)
		{
			return new Interfaces.MessageBroker.Message
			{
				DataSource = datasource,
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = domainType
			};
		}
	}
}