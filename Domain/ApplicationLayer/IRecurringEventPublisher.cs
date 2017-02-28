using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IRecurringEventPublisher
	{
		void PublishDaily(IEvent @event);
		void PublishHourly(IEvent @event);
		void PublishMinutely(IEvent @event); // http://english.stackexchange.com/questions/3091/weekly-daily-hourly-minutely
		void StopPublishingAll();
		void StopPublishingForTenantsExcept(IEnumerable<string> excludedTenants);
	}
}
