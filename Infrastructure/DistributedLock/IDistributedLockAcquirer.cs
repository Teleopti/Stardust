using System;

namespace Teleopti.Ccc.Infrastructure.DistributedLock
{
	public interface IDistributedLockAcquirer
	{
		IDisposable LockForTypeOf(object lockObject);
	}
}