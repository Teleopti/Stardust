using System;

namespace Teleopti.Ccc.Domain.DistributedLock
{
	public interface IDistributedLockAcquirer
	{
		IDisposable LockForTypeOf(object lockObject);
	}
}