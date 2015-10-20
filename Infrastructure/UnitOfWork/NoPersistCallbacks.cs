using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NoPersistCallbacks : ICurrentPersistCallbacks
	{
		public IEnumerable<IPersistCallback> Current()
		{
			yield break;
		}
	}
}