using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IDenormalizer
	{
		void Execute(IRunSql runSql, IEnumerable<IRootChangeInfo> modifiedRoots);
	}
}