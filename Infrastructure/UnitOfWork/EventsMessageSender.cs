using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class EventsMessageSender : ITransactionHook
	{
		private readonly IEventPopulatingPublisher _publisher;

		public EventsMessageSender(IEventPopulatingPublisher publisher)
		{
			_publisher = publisher;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var withEvents = modifiedRoots
				.Where(m => m.Root is IAggregateRootWithEvents)
				.Select(m => new
				{
					Root = m.Root as IAggregateRootWithEvents,
					m.Status
				})
				.ToArray();
			if (!withEvents.Any()) return;

			var events = withEvents
				.SelectMany(e =>
				{
					if (e.Status == DomainUpdateType.Delete)
						e.Root.NotifyDelete();
					e.Root.NotifyTransactionComplete(e.Status);
					return e.Root.PopAllEvents();
				})
				.ToArray();

			_publisher.Publish(events);
		}
		
	}
}