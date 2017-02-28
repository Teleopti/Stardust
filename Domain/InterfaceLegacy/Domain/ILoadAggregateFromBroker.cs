using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ILoadAggregateFromBroker<T>
	{
		T LoadAggregate(Guid id);
	}
}