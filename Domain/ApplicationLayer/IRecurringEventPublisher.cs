using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IRecurringEventPublisher
	{
		void PublishHourly(IEvent @event);
		void StopPublishingForCurrentTenant();
		IEnumerable<string> TenantsWithRecurringJobs();
	}
}