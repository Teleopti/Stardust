using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.DomainTest.Common
{
	public class ScheduleChangedEventDetector:IHandleEvent<ScheduleChangedEvent>
	{
		private List<ScheduleChangedEvent> _events = new List<ScheduleChangedEvent>();

		public void Handle(ScheduleChangedEvent @event)
		{
			_events.Add(@event);
		}

		public void Reset()
		{
			_events.Clear();
		}

		public IEnumerable<ScheduleChangedEvent> GetEvents()
		{
			return _events.ToArray();
		}
	}
}