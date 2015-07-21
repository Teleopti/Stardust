﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeEventPublisher : IEventPublisher, ISyncEventPublisher
	{
		public IEnumerable<IEvent> PublishedEvents { get { return queuedEvents.ToArray(); }}
		private ConcurrentQueue<IEvent> queuedEvents = new ConcurrentQueue<IEvent>();

		public void Clear()
		{
			queuedEvents = new ConcurrentQueue<IEvent>();
		}

		public void Publish(params IEvent[] events)
		{
			events.ForEach(queuedEvents.Enqueue);
		}
	}
}