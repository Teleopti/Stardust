using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IPreCommitHook
	{
		void BeforeCommit(IEnumerable<IRootChangeInfo> modifiedRoots);
	}
}