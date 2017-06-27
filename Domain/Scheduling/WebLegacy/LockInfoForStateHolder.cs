using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class LockInfoForStateHolder
	{
		public LockInfoForStateHolder(IGridlockManager gridlockManager, IEnumerable<LockInfo> locks)
		{
			GridlockManager = gridlockManager;
			Locks = locks;
		}

		public IGridlockManager GridlockManager { get; }
		public IEnumerable<LockInfo> Locks { get; }
	}
}