using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IPreCommitHook
	{
		void BeforeCommit(object root, IEnumerable<string> propertyNames, object[] currentState);
	}
}