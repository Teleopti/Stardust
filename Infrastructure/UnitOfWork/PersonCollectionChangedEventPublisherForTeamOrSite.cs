using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class PersonCollectionChangedEventPublisherForTeamOrSite : IPersistCallback
	{
		private readonly IEventPopulatingPublisher _eventsPublisher;
		private readonly ICurrentBusinessUnit _businessUnit;

		private readonly IEnumerable<Type> _otherTriggerInterfaces = new List<Type>
			{
				typeof (ITeam),
				typeof (ISite),
			};

		public PersonCollectionChangedEventPublisherForTeamOrSite(IEventPopulatingPublisher eventsPublisher, ICurrentBusinessUnit businessUnit)
		{
			_eventsPublisher = eventsPublisher;
			_businessUnit = businessUnit;
		}

		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			// we are signed in with a transient BU and cant publish events
			var bu = _businessUnit.Current();
			if (bu == null || !bu.Id.HasValue)
				return;

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