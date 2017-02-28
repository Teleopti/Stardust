using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IProxyForId<T>
	{
		T Load(Guid id);
	}
}