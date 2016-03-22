using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class TenantTickEventPublisher
	{
		private readonly AllTenantRecurringEventPublisher _publisher;
		private readonly INow _now;
		private DateTime? _nextPublish;

		public TenantTickEventPublisher(
			AllTenantRecurringEventPublisher publisher,
			INow now)
		{
			_publisher = publisher;
			_now = now;
		}

		public void EnsurePublishings()
		{
			if (_nextPublish == null)
				_publisher.RemoveAllPublishings();

			var nextPublish = _nextPublish ?? _now.UtcDateTime();
			if (_now.UtcDateTime() < nextPublish)
				return;
			_nextPublish = _now.UtcDateTime().AddMinutes(10);

			_publisher.RemovePublishingsOfRemovedTenants();

			_publisher.PublishMinutely(new TenantMinuteTickEvent());
			_publisher.PublishHourly(new TenantHourTickEvent());
		}
	}

}