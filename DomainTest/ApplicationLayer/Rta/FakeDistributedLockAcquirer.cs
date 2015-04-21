using System;
using System.Threading;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.DistributedLock;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class FakeDistributedLockAcquirer : IDistributedLockAcquirer
	{
		public IDisposable LockForTypeOf(object lockObject)
		{
			var type = lockObject.GetType();
			Monitor.Enter(type);
			return new GenericDisposable(() => Monitor.Exit(type));
		}
	}
}