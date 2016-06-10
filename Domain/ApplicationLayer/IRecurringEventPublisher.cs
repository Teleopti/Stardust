using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IRecurringEventPublisher
	{
		void PublishDaily(IEvent @event, TimeZoneInfo timeZone);
		void PublishHourly(IEvent @event);
		void PublishMinutely(IEvent @event); // http://english.stackexchange.com/questions/3091/weekly-daily-hourly-minutely
		IEnumerable<string> TenantsWithRecurringJobs();
		void StopPublishingForCurrentTenant();
		void StopPublishingAll();
		void StopPublishingForEvent<T>() where T : IEvent;
	}
}
