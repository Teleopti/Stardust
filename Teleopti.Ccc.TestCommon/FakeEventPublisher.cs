using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeEventPublisher : IEventPublisher, ISyncEventPublisher
	{
		public IEnumerable<IEvent> PublishedEvents { get { return events.ToArray(); }}
		//public IList<IEvent> PublishedEvents { get { return events.Reverse().ToList(); } }
		private ConcurrentQueue<IEvent> events = new ConcurrentQueue<IEvent>();

		public void Clear()
		{
			events = new ConcurrentQueue<IEvent>();
		}

		public void Publish(IEvent @event)
		{
			events.Enqueue(@event);
		}
	}
}