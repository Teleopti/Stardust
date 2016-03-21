using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IRecurringEventPublisher
	{
		void PublishHourly(IEvent @event);
		IEnumerable<string> TenantsWithRecurringJobs();
		void StopPublishingForCurrentTenant();
		void StopPublishingAll();
	}
}