using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public class ScheduleChangeProcessor :
		IRunOnHangfire,
		IHandleEvent<ScheduleChangedEvent>,
		IHandleEventOnQueue<ScheduleChangedEvent>,
		IHandleEvent<TenantMinuteTickEvent>,
		IHandleEvent<TenantDayTickEvent>
	{
		private readonly IDistributedLockAcquirer _distributedLock;
		private readonly CurrentScheduleReadModelUpdater _updater;
		private readonly IActivityChangeCheckerFromScheduleChangeProcessor _checker;
		private readonly IRtaEventStoreSynchronizer _synchronizer;

		public ScheduleChangeProcessor(
			IDistributedLockAcquirer distributedLock,
			CurrentScheduleReadModelUpdater updater,
			IActivityChangeCheckerFromScheduleChangeProcessor checker,
			IRtaEventStoreSynchronizer synchronizer)
		{
			_distributedLock = distributedLock;
			_updater = updater;
			_checker = checker;
			_synchronizer = synchronizer;
		}

		[Attempts(7)]
		public void Handle(ScheduleChangedEvent @event) =>
			_updater.Invalidate(@event.PersonId, @event.StartDateTime, @event.EndDateTime);

		public string QueueTo(ScheduleChangedEvent @event) =>
			_updater.ShouldInvalidate(@event.StartDateTime, @event.EndDateTime) ? Queues.CriticalScheduleChangesToday : null;

		public void Handle(TenantMinuteTickEvent @event)
		{
			_distributedLock.TryLockForTypeOf(_updater, () => { _updater.UpdateInvalids(); });
			_distributedLock.TryLockForTypeOf(_checker, () => { _checker.CheckForActivityChanges(); });
			_distributedLock.TryLockForTypeOf(_synchronizer, () => { _synchronizer.Synchronize(); }); 
		}

		[Attempts(10)]
		public virtual void Handle(TenantDayTickEvent @event) =>
			_updater.InvalidateAll();
	}
}