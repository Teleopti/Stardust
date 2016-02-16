using System;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class NotificationCreator : INotificationCreator
	{
		public Message Create(string datasource, Guid businessUnitId, string domainType)
		{
			return new Message
			{
				DataSource = datasource,
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = domainType
			};
		}
	}
}