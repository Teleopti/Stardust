using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class LockInfoForStateHolder
	{
		public LockInfoForStateHolder(IGridlockManager gridlockManager, IEnumerable<LockInfo> locks)
		{
			GridlockManager = gridlockManager;
			Locks = locks ?? Enumerable.Empty<LockInfo>();
		}

		public IGridlockManager GridlockManager { get; }
		public IEnumerable<LockInfo> Locks { get; }
	}
}