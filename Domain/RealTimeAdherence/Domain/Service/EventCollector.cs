using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
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

		public IEnumerable<IEvent> Pop()
		{
			var result = _events.ToArray();
			_events.Clear();
			return result;
		}
	}
}