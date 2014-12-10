using System;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface INotificationCreator
	{
		Notification Create(string datasource, Guid businessUnitId, string domainType);
	}
}