using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IRecurringEventPublisher
	{
		void PublishHourly(string id, IEvent @event);
		void StopPublishing(string id);
		IEnumerable<string> RecurringPublishingIds();
	}
}