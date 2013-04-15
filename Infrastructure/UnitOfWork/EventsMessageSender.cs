using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class EventsMessageSender : IMessageSender
	{
		private readonly IEventsPublisher _publisher;

		public EventsMessageSender(IEventsPublisher publisher) {
			_publisher = publisher;
		}

		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var events = (from i in modifiedRoots
			              let r = i.Root
			              let withEvents = r as IAggregateRootWithEvents
			              where withEvents != null
			              let es = withEvents.PopAllEvents()
			              where es != null
			              from e in es
			              select e)
				.ToArray();
			_publisher.Publish(events);
		}
	}
}