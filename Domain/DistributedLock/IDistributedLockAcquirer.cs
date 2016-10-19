using System;

namespace Teleopti.Ccc.Domain.DistributedLock
{
	public interface IDistributedLockAcquirer
	{
		IDisposable LockForTypeOf(object lockObject);
		void TryLockForTypeOf(object lockObject, Action action);
		IDisposable LockForGuid(object lockObject, Guid guid);
		void TryLockForGuid(object lockObject, Guid guid, Action action);
	}
}