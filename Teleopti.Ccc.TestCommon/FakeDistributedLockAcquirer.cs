using System;
using System.Threading;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.DistributedLock;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeDistributedLockAcquirer : IDistributedLockAcquirer
	{
		public IDisposable LockForTypeOf(object lockObject)
		{
			return @lock(lockObject.GetType().Name);
		}

		public void TryLockForTypeOf(object lockObject, Action action)
		{
			tryLock(lockObject.GetType().Name, action);
		}

		public IDisposable LockForGuid(object lockObject, Guid guid)
		{
			return @lock(lockObject.GetType() + guid.ToString());
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

		private static IDisposable @lock(string name)
		{
			Monitor.Enter(name);
			return new GenericDisposable(() => Monitor.Exit(name));
		}
	}
}