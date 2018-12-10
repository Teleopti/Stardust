using System;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Wfm.Adherence.States
{
	public interface INotificationCreator
	{
		Message Create(string datasource, Guid businessUnitId, string domainType);
	}
}