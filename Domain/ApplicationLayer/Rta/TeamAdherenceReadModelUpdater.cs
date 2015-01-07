using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	[UseOnToggle(Toggles.RTA_NoBroker_31237)]
	public class TeamAdherenceReadModelUpdater : IHandleEvent<PersonInAdherenceEvent>, IHandleEvent<PersonOutOfAdherenceEvent>
	{
		private readonly ITeamAdherencePersister _persister;

		public TeamAdherenceReadModelUpdater(ITeamAdherencePersister persister)
		{
			_persister = persister;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			var current = _persister.Get(@event.TeamId);
			if (current != null)
			{
				current.AgentsOutOfAdherence = Math.Max(current.AgentsOutOfAdherence - 1, 0);
				current.SiteId = @event.SiteId;
				_persister.Persist(current);
			}
			else
			{
				_persister.Persist(new TeamAdherenceReadModel() { TeamId = @event.TeamId, SiteId = @event.SiteId});

			}
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			var current = _persister.Get(@event.TeamId);
			if (current != null)
			{
				current.AgentsOutOfAdherence++;
				current.SiteId = @event.SiteId;
				_persister.Persist(current);
			}
			else
			{
				_persister.Persist(new TeamAdherenceReadModel() { TeamId = @event.TeamId, AgentsOutOfAdherence = 1,SiteId = @event.SiteId});
			}
		}
	}
}