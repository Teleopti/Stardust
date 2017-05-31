using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface ICurrentPreCommitHooks
	{
		IEnumerable<IPreCommitHook> Current();
	}
}