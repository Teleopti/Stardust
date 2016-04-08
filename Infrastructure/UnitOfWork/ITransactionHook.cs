using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface ITransactionHook
	{
		void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots);
	}
}