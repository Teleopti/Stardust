using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class EventCollector : IEventPublisher
	{
		private readonly IList<IEvent> _events = new List<IEvent>();
		private readonly ICurrentEventPublisher _publisher;

		public EventCollector(ICurrentEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Publish(params IEvent[] events)
		{
			events.ForEach(_events.Add);
		}

		public void PublishTransitions()
		{
			var publisher = _publisher.Current();
			// of duplicate events, publish the first, which IS the first in time because..
			// they are already in order of time because they are currently collected in that order by the callers...
			var toPublish = from e in _events
				group e by e.GetType()
				into x
				select x.First();
			toPublish.ForEach(@event => publisher.Publish(@event));
		}

	}
}