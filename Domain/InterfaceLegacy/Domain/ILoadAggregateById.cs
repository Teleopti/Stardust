using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ILoadAggregateById<T> : ILoadAggregateByTypedId<T, Guid>
	{
	}
}