using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityChangeProcessor : 
		IHandleEvent<TenantMinuteTickEvent>, 
		IRunOnHangfire
	{
		private readonly ActivityChangeChecker _checker;
		private readonly IDistributedLockAcquirer _distributedLock;

		public ActivityChangeProcessor(
			ActivityChangeChecker checker,
			IDistributedLockAcquirer distributedLock
			)
		{
			_checker = checker;
			_distributedLock = distributedLock;
		}

		public void Handle(TenantMinuteTickEvent @event)
		{
			_distributedLock.TryLockForTypeOf(this, _checker.CheckForActivityChanges);
		}
		
	}
}