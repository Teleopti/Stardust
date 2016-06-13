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

		public void EnsurePublishings()
		{
			_allTenantRecurringEventPublisher.RemovePublishingsOfRemovedTenants();

			_allTenantRecurringEventPublisher.PublishMinutely(new TenantMinuteTickEvent());
			_allTenantRecurringEventPublisher.PublishHourly(new TenantHourTickEvent());
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