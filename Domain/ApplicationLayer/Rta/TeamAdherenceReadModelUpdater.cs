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
		public void Handle(PersonInAdherenceEvent @event)
		{
			var current = _persister.Get(@event.TeamId);
			if (current != null)
			{
				current.AgentsOutOfAdherence = Math.Max(current.AgentsOutOfAdherence - 1, 0);
				_persister.Persist(current);
			}
			else
			{
				_persister.Persist(new TeamAdherenceReadModel() { TeamId = @event.TeamId });

			}
		}

		[ReadModelUnitOfWork]
		public void Handle(PersonOutOfAdherenceEvent @event)
		{
			var current = _persister.Get(@event.TeamId);
			if (current != null)
			{
				current.AgentsOutOfAdherence++;
				_persister.Persist(current);
			}
			else
			{
				_persister.Persist(new TeamAdherenceReadModel() { TeamId = @event.TeamId, AgentsOutOfAdherence = 1 });
			}
		}
	}

}