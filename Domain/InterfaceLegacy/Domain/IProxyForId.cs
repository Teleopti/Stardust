using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IProxyForId<T>
	{
		T Load(Guid id);
	}
}