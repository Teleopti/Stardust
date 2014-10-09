using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	public class FakeEventsPublisher : IEventsPublisher
	{
		public IList<IEvent> PublishedEvents = new List<IEvent>();

		public void Publish(IEnumerable<IEvent> events)
		{
			events.ForEach(PublishedEvents.Add);
		}
	}
}