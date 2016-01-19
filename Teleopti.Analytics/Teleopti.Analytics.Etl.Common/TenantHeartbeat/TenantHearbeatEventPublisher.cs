using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.TenantHeartbeat
{
	public class TenantHearbeatEventPublisher
	{
		private readonly AllTenantRecurringEventPublisher _publisher;
		private readonly INow _now;
		private DateTime? _nextPublish;

		public TenantHearbeatEventPublisher(
			AllTenantRecurringEventPublisher publisher,
			INow now)
		{
			_publisher = publisher;
			_now = now;
		}

		public void Tick()
		{
			var nextPublish = _nextPublish ?? _now.UtcDateTime();
			if (_now.UtcDateTime() < nextPublish)
				return;
			_nextPublish = _now.UtcDateTime().AddMinutes(10);

			_publisher.Publish(new TenantHearbeatEvent());
		}
	}

}