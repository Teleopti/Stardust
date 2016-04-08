using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NoTransactionHooks : ICurrentTransactionHooks
	{
		public IEnumerable<ITransactionHook> Current()
		{
			yield break;
		}
	}
}