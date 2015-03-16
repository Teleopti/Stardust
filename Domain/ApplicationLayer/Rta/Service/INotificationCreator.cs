using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface INotificationCreator
	{
		Interfaces.MessageBroker.Notification Create(string datasource, Guid businessUnitId, string domainType);
	}
}