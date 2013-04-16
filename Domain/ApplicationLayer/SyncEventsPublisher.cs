using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class SyncEventsPublisher : IEventsPublisher
	{
		private readonly IEventPublisher _eventPublisher;

		public SyncEventsPublisher(IEventPublisher eventPublisher) { _eventPublisher = eventPublisher; }

		public void Publish(IEnumerable<IEvent> events)
		{
			events.ForEach(e => _eventPublisher.Publish(e));
		}
	}
}