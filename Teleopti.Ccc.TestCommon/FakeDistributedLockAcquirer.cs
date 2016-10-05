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
			var type = lockObject.GetType();
			Monitor.Enter(type);
			return new GenericDisposable(() => Monitor.Exit(type));
		}

		public void TryLockForTypeOf(object lockObject, Action action)
		{
			var type = lockObject.GetType();
			if (!Monitor.TryEnter(type)) return;
			action.Invoke();
			Monitor.Exit(type);
		}

		public IDisposable LockForGuid(object lockObject, Guid guid)
		{
			var lockString = lockObject.GetType()+ guid.ToString();
			Monitor.Enter(lockString);
			return new GenericDisposable(() => Monitor.Exit(lockString));
		}
	}
}