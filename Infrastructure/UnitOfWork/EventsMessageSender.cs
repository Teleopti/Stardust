using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
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

		public void Execute(IMessageBrokerIdentifier messageBrokerIdentifier, IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var withEvents = modifiedRoots.Select(m => m.Root).OfType<IAggregateRootWithEvents>();
			if (!withEvents.Any()) return;

			var initiatorId = Guid.Empty;

			if (messageBrokerIdentifier != null)
				initiatorId = messageBrokerIdentifier.InstanceId;

			var events = withEvents.SelectMany(e => e.PopAllEvents()).ToArray().ForEach(e => e.InitiatorId = initiatorId);
			 _publisher.Publish(events);
		}
	}
}