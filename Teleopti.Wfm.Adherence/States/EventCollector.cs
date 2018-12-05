using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.States
{
	public class EventCollector : IEventPublisher
	{
		private readonly IList<IEvent> _events = new List<IEvent>();

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