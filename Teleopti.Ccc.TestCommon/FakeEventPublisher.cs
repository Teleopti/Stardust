using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeEventPublisher : IEventPublisher
	{
		public IEnumerable<IEvent> PublishedEvents => queuedEvents.ToArray();
		private ConcurrentQueue<IEvent> queuedEvents = new ConcurrentQueue<IEvent>();

		public void Clear()
		{
			queuedEvents = new ConcurrentQueue<IEvent>();
		}

		public virtual void Publish(params IEvent[] events)
		{
			events.ForEach(queuedEvents.Enqueue);
		}
	}
}