using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityChangeProcessor :
		IHandleEvent<TenantMinuteTickEvent>,
		IRunOnHangfire
	{
		private readonly IActivityChangeChecker _activityChangeChecker;
		private readonly IDistributedLockAcquirer _distributedLock;

		public ActivityChangeProcessor(
			IActivityChangeChecker activityChangeChecker,
			IDistributedLockAcquirer distributedLock
			)
		{
			_activityChangeChecker = activityChangeChecker;
			_distributedLock = distributedLock;
		}

		[RecurringJob]
		public void Handle(TenantMinuteTickEvent @event)
		{
			_distributedLock.TryLockForTypeOf(this, _activityChangeChecker.CheckForActivityChanges);
		}
	}
}

