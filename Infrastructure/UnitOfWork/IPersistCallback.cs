using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IPersistCallback
	{
		void AdditionalFlush(IEnumerable<IRootChangeInfo> modifiedRoots);
		void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots);
	}
}