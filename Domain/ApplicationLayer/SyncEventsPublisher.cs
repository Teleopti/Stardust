using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class SyncEventsPublisher : IEventsPublisher
	{
		private readonly IEventPopulatingPublisher _eventPopulatingPublisher;

		public SyncEventsPublisher(IEventPopulatingPublisher eventPopulatingPublisher) { _eventPopulatingPublisher = eventPopulatingPublisher; }

		public void Publish(IEnumerable<IEvent> events)
		{
			events.ForEach(e => _eventPopulatingPublisher.Publish(e));
		}
	}
}