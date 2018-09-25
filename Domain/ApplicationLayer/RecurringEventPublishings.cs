using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class RecurringEventPublishings
	{
		private readonly AllTenantRecurringEventPublisher _allTenantRecurringEventPublisher;
		private readonly IRecurringEventPublisher _recurringEventPublisher;

		public RecurringEventPublishings(
			AllTenantRecurringEventPublisher allTenantRecurringEventPublisher,
			IRecurringEventPublisher recurringEventPublisher)
		{
			_allTenantRecurringEventPublisher = allTenantRecurringEventPublisher;
			_recurringEventPublisher = recurringEventPublisher;
		}

		public void UpdatePublishings()
		{
			_allTenantRecurringEventPublisher.RemovePublishingsOfRemovedTenants();
			_recurringEventPublisher.PublishHourly(new CleanFailedQueue());
			_allTenantRecurringEventPublisher.PublishMinutely(new TenantMinuteTickEvent());
			_allTenantRecurringEventPublisher.PublishHourly(new TenantHourTickEvent());
			_allTenantRecurringEventPublisher.PublishDaily(new TenantDayTickEvent());
		}

		public void WithPublishingsForTest(Action action)
		{
			_allTenantRecurringEventPublisher.RemoveAllPublishings();
			_allTenantRecurringEventPublisher.PublishMinutely(new TenantMinuteTickEvent());
			_allTenantRecurringEventPublisher.PublishHourly(new TenantHourTickEvent());
			_allTenantRecurringEventPublisher.PublishDaily(new TenantDayTickEvent());

			action.Invoke();

			_allTenantRecurringEventPublisher.RemoveAllPublishings();
		}
	}
}