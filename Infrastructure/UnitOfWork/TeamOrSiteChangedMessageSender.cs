using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class TeamOrSiteChangedMessageSender : IMessageSender
	{
		private readonly IEventPopulatingPublisher _eventsPublisher;

		private readonly IEnumerable<Type> _otherTriggerInterfaces = new List<Type>
			{
				typeof (ITeam),
				typeof (ISite),
			};

		public TeamOrSiteChangedMessageSender(IEventPopulatingPublisher eventsPublisher)
		{
			_eventsPublisher = eventsPublisher;
		}

		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var affectedInterfaces = from r in modifiedRoots
			                         let t = r.Root.GetType()
			                         where _otherTriggerInterfaces.Any(ti => ti.IsAssignableFrom(t))
			                         select r.Root;

			if (!affectedInterfaces.Any()) return;

			var message = new PersonCollectionChangedEvent{SerializedPeople = Guid.Empty.ToString()};
			_eventsPublisher.Publish(message);
		}
	}
}