using System;

namespace Teleopti.Interfaces.Domain
{
	public interface ILoadAggregateById<T> : ILoadAggregateByTypedId<T, Guid>
	{
	}
}