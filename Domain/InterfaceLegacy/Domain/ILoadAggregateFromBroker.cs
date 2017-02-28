using System;

namespace Teleopti.Interfaces.Domain
{
	public interface ILoadAggregateFromBroker<T>
	{
		T LoadAggregate(Guid id);
	}
}