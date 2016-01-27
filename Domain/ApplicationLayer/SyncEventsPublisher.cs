using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class SyncEventsPublisher
	{
		private readonly IEventPopulatingPublisher _eventPopulatingPublisher;

		public SyncEventsPublisher(IEventPopulatingPublisher eventPopulatingPublisher) { _eventPopulatingPublisher = eventPopulatingPublisher; }

		public void Publish(IEnumerable<IEvent> events)
		{
			_eventPopulatingPublisher.Publish(events.ToArray());
		}
	}
}