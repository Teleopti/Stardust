﻿using System.Collections.Generic;
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
			var withEvents = modifiedRoots.Select(m => m.Root).OfType<IAggregateRootWithEvents>();
			if (!withEvents.Any()) return;

			var events = withEvents.SelectMany(e => e.PopAllEvents(_now)).ToArray();
			_publisher.Publish(events);
		}
		
	}
}