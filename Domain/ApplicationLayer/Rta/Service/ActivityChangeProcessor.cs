using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityChangeProcessor :
		IHandleEvent<TenantMinuteTickEvent>,
		IRunOnHangfire
	{
		private readonly ActivityChangeChecker _activityChangeChecker;
		private readonly IDistributedLockAcquirer _distributedLock;

		public ActivityChangeProcessor(
			ActivityChangeChecker activityChangeChecker,
			IDistributedLockAcquirer distributedLock
			)
		{
			_activityChangeChecker = activityChangeChecker;
			_distributedLock = distributedLock;
		}

		[LogInfo]
		public virtual void Handle(TenantMinuteTickEvent @event)
		{
			_distributedLock.TryLockForTypeOf(this, _activityChangeChecker.CheckForActivityChanges);
		}
	}
}

