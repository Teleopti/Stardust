using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class EventsMessageSender : IPersistCallback
	{
		private readonly IEventPublisher _publisher;

		public EventsMessageSender(IEventPublisher publisher) {
			_publisher = publisher;
		}

		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var withEvents = modifiedRoots.Select(m => m.Root).OfType<IAggregateRootWithEvents>();
			if (!withEvents.Any()) return;

			var events = withEvents.SelectMany(e => e.PopAllEvents()).ToArray();
			_publisher.Publish(events);
		}

	}
}