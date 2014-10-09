using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	public class FakeEventsPublisher : IEventPublisher
	{
		public IList<IEvent> PublishedEvents = new List<IEvent>();

		public void Publish(IEvent @event)
		{
			PublishedEvents.Add(@event);
		}
	}
}