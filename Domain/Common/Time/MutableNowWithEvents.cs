using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public class MutableNowWithEvents : MutableNow
	{
		private readonly IEventPublisher _publisher;

		public MutableNowWithEvents(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public override void Is(DateTime? utc)
		{
			var time = new TimePassingSimulator(UtcDateTime(), utc.GetValueOrDefault());
			
			base.Is(utc);

			time.IfDayPassed(() => { _publisher.Publish(new TenantDayTickEvent()); });
			time.IfHourPassed(() => { _publisher.Publish(new TenantHourTickEvent()); });
			time.IfMinutePassed(() => { _publisher.Publish(new TenantMinuteTickEvent()); });
		}
	}
}