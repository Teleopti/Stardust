using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentPreCommitHooks : ICurrentPreCommitHooks
	{
		private readonly IEnumerable<IPreCommitHook> _beforeCommits;

		public CurrentPreCommitHooks(IEnumerable<IPreCommitHook> beforeCommits)
		{
			_beforeCommits = beforeCommits;
		}

		public IEnumerable<IPreCommitHook> Current()
		{
			return _beforeCommits;
		}
	}
}