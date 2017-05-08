using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class LegacyFakeEventPublisher : IEventPublisher
	{
		private readonly ConcurrentQueue<IEvent> queuedEvents = new ConcurrentQueue<IEvent>();

		public IEnumerable<IEvent> PublishedEvents => queuedEvents.ToArray();

		public void Publish(params IEvent[] events)
		{
			events.ForEach(queuedEvents.Enqueue);
		}
	}
}