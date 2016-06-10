using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class TenantTickEventPublisher
	{
		private readonly AllTenantRecurringEventPublisher _allTenantRecurringEventPublisher;
		private readonly INow _now;
		private DateTime? _nextPublish;

		public TenantTickEventPublisher(
			AllTenantRecurringEventPublisher allTenantRecurringEventPublisher,
			INow now)
		{
			_allTenantRecurringEventPublisher = allTenantRecurringEventPublisher;
			_now = now;
		}

		public void EnsurePublishings()
		{
			if (_nextPublish == null)
			{
				_allTenantRecurringEventPublisher.RemovePublishingForEvent<TenantMinuteTickEvent>();
				_allTenantRecurringEventPublisher.RemovePublishingForEvent<TenantHourTickEvent>();
			}

			var nextPublish = _nextPublish ?? _now.UtcDateTime();
			if (_now.UtcDateTime() < nextPublish)
				return;
			_nextPublish = _now.UtcDateTime().AddMinutes(10);

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