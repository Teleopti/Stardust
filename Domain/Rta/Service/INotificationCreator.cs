using System;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public interface INotificationCreator
	{
		Message Create(string datasource, Guid businessUnitId, string domainType);
	}
}