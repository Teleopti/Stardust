﻿using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
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

		public ScheduleChangeProcessor(
			IDistributedLockAcquirer distributedLock, 
			CurrentScheduleReadModelUpdater updater,
			IActivityChangeCheckerFromScheduleChangeProcessor checker)
		{
			_distributedLock = distributedLock;
			_updater = updater;
			_checker = checker;
		}

		public void Handle(ScheduleChangedEvent @event)
		{
			_updater.Invalidate(@event.PersonId, @event.StartDateTime, @event.EndDateTime);
		}

		public string QueueTo(ScheduleChangedEvent @event)
		{
			return _updater.ShouldInvalidate(@event.StartDateTime, @event.EndDateTime) ? 
				Queues.ScheduleChangesToday : 
				null;
		}

		public void Handle(TenantMinuteTickEvent @event)
		{
			_distributedLock.TryLockForTypeOf(_updater, () =>
			{
				_updater.UpdateInvalids();
			});

			_distributedLock.TryLockForTypeOf(_checker, () =>
			{
				_checker.CheckForActivityChanges();
			});
		}

		[Attempts(3)]
		public virtual void Handle(TenantDayTickEvent @event)
		{
			_updater.InvalidateAll();
		}
	}

}
