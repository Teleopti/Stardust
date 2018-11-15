using System;
using System.Collections.Concurrent;
using System.Threading;
using Teleopti.Ccc.Domain.DistributedLock;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeDistributedLockAcquirer : IDistributedLockAcquirer
	{
		private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

		public void TryLockForTypeOf(object lockObject, Action action) =>
			tryLock(lockObject.GetType().Name, action);

		public void TryLockForTypeOfAnd(object lockObject, string extra, Action action) =>
			tryLock(lockObject.GetType().Name + extra, action);

		private void tryLock(string name, Action action)
		{
			var @lock = _locks.GetOrAdd(name, x => new object());
			if (!Monitor.TryEnter(@lock)) return;
			try
			{
				action.Invoke();
			}
			finally
			{
				Monitor.Exit(@lock);
			}
		}
	}
}