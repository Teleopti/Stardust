using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.TickEvent
{
	public class HourlyTickEventPublisher
	{
		private readonly AllTenantRecurringEventPublisher _publisher;
		private readonly INow _now;
		private DateTime? _nextPublish;

		public HourlyTickEventPublisher(
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

			_publisher.Publish(new HourlyTickEvent());
		}
	}

}