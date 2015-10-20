using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface ICurrentPersistCallbacks
	{
		IEnumerable<IPersistCallback> Current();
	}
}