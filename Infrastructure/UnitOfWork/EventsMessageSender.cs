using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class EventsMessageSender : ITransactionHook
	{
		private readonly IEventPopulatingPublisher _publisher;
		private readonly INow _now;

		public EventsMessageSender(
			IEventPopulatingPublisher publisher,
			INow now
		)
		{
			_publisher = publisher;
			_now = now;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var withEvents = modifiedRoots
				.Where(m => m.Root is IAggregateRoot_Events)
				.Select(m => new
				{
					Root = m.Root as IAggregateRoot_Events,
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
					return e.Root.PopAllEvents(new Injected(_now));
				})
				.ToArray();

			if (events.Length > 0)
				_publisher.Publish(events);
		}
	}
}