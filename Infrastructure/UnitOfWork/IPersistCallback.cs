using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IPersistCallback
	{
		void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots);
		void AfterCommit(IEnumerable<IRootChangeInfo> modifiedRoots);
	}
}