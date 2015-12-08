using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class OptimizeScheduleEventPublisher : IPersistCallback
	{
		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{

		}
	}
}