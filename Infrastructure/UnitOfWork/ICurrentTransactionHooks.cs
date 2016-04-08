using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface ICurrentTransactionHooks
	{
		IEnumerable<ITransactionHook> Current();
	}
}