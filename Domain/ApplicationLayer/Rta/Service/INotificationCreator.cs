using System;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface INotificationCreator
	{
		Message Create(string datasource, Guid businessUnitId, string domainType);
	}
}