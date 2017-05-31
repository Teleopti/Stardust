using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NoPreCommitHooks : ICurrentPreCommitHooks
	{
		public IEnumerable<IPreCommitHook> Current()
		{
			yield break;
		}
	}
}