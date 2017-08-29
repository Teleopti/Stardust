using System;
using System.Threading;
using Teleopti.Ccc.Domain.DistributedLock;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeDistributedLockAcquirer : IDistributedLockAcquirer
	{
		public void TryLockForTypeOf(object lockObject, Action action)
		{
			tryLock(lockObject.GetType().Name, action);
		}
		
		public void TryLockForTypeOfAnd(object lockObject, string extra, Action action)
		{
			tryLock(lockObject.GetType().Name + extra, action);
		}

		private static void tryLock(string name, Action action)
		{
			if (!Monitor.TryEnter(name)) return;
			action.Invoke();
			Monitor.Exit(name);
		}
	}
}