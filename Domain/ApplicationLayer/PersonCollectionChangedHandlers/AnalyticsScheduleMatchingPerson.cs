using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	[EnabledBy(Toggles.ETL_FixScheduleForPersonPeriod_41393)]
	public class AnalyticsScheduleMatchingPerson : IHandleEvent<AnalyticsPersonPeriodRangeChangedEvent>, IRunOnHangfire
	{
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;

		public AnalyticsScheduleMatchingPerson(IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository, IAnalyticsScheduleRepository analyticsScheduleRepository, IDistributedLockAcquirer distributedLockAcquirer)
		{
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsScheduleRepository = analyticsScheduleRepository;
			_distributedLockAcquirer = distributedLockAcquirer;
		}

		[Attempts(10)]
		[LogInfo]
		public virtual void Handle(AnalyticsPersonPeriodRangeChangedEvent @event)
		{
			var didRun = false;
			_distributedLockAcquirer.TryLockForTypeOf(this, () =>
			{
				didRun = true;
				foreach (var personId in @event.PersonIdCollection)
					UpdateUnlinked(personId);
			});
			if (!didRun)
				throw new Exception("Another handler is running currently, explicitly failing to retry!");
		}

		[AnalyticsUnitOfWork]
		protected virtual void UpdateUnlinked(Guid personId)
		{
			var analyticsPersonPeriods = _analyticsPersonPeriodRepository.GetPersonPeriods(personId);
			var personPeriodIds = analyticsPersonPeriods.Select(x => x.PersonId).ToArray();
			_analyticsScheduleRepository.UpdateUnlinkedPersonids(personPeriodIds);
		}
	}
}