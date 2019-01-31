using System;

namespace Teleopti.Ccc.Domain.DistributedLock
{
	public interface IDistributedLockAcquirer
	{
		void TryLockForTypeOf(object lockObject, Action action);
		void TryLockForTypeOfAnd(object lockObject, string extra, Action action);
	}
}