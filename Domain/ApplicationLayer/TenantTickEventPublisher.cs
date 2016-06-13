using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class TenantTickEventPublisher
	{
		private readonly AllTenantRecurringEventPublisher _allTenantRecurringEventPublisher;

		public TenantTickEventPublisher(AllTenantRecurringEventPublisher allTenantRecurringEventPublisher)
		{
			_allTenantRecurringEventPublisher = allTenantRecurringEventPublisher;
		}

		public void PublishRecurringJobs()
		{
			_allTenantRecurringEventPublisher.PublishMinutely(new TenantMinuteTickEvent());
			_allTenantRecurringEventPublisher.PublishHourly(new TenantHourTickEvent());
		}

		public void RemovePublishingsOfRemovedTenants()
		{
			_allTenantRecurringEventPublisher.RemovePublishingsOfRemovedTenants();
		}

		public void WithPublishingsForTest(Action action)
		{
			_allTenantRecurringEventPublisher.RemoveAllPublishings();
			_allTenantRecurringEventPublisher.PublishMinutely(new TenantMinuteTickEvent());
			_allTenantRecurringEventPublisher.PublishHourly(new TenantHourTickEvent());

			action.Invoke();

			_allTenantRecurringEventPublisher.RemoveAllPublishings();

		}
	}

}