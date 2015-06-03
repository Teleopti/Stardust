using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface INotificationCreator
	{
		Interfaces.MessageBroker.Message Create(string datasource, Guid businessUnitId, string domainType);
	}
}