using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class EventsMessageSender : ITransactionHook
	{
		private readonly IEventPopulatingPublisher _publisher;
		private readonly INow _now;

		public EventsMessageSender(IEventPopulatingPublisher publisher, INow now)
		{
			_publisher = publisher;
			_now = now;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			//var withEvents = modifiedRoots.Select(m => m.Root).OfType<IAggregateRootWithEvents>();
		    var withEvents = modifiedRoots.Where(m => m.Root is IAggregateRootWithEvents).Select(m => new EventPublishingRoot
		    {
                DomainUpdateType = m.Status,
                RootWithEvents = m.Root as IAggregateRootWithEvents
            });
			if (!withEvents.Any()) return;

			var events = withEvents.SelectMany(e => e.RootWithEvents.PopAllEvents(_now, e.DomainUpdateType)).ToArray();
			_publisher.Publish(events);
		}

	    private class EventPublishingRoot
        {
	        public IAggregateRootWithEvents RootWithEvents { get; set; }
	        public DomainUpdateType DomainUpdateType { get; set; }
	    }
	}
}