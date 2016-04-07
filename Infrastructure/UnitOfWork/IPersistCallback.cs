using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IPersistCallback
	{
		void AdditionalFlush(IUnitOfWork unitOfWork, IEnumerable<IRootChangeInfo> modifiedRoots);
		void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots);
	}
}