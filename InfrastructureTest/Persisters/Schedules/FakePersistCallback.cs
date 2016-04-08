using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class FakePersistCallback : IPersistCallback
	{
		public IEnumerable<IRootChangeInfo> AfterFlushInvokedWith;
		public IEnumerable<object> ModifiedRoots;

		public void AfterCommit(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			AfterFlushInvokedWith = modifiedRoots;
			ModifiedRoots = AfterFlushInvokedWith.Select(x => x.Root);
		}
		
		public void Clear()
		{
			AfterFlushInvokedWith = null;
			ModifiedRoots = null;
		}
	}
}