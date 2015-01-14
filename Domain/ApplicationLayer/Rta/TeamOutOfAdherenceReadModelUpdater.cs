using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	[UseOnToggle(Toggles.RTA_NoBroker_31237)]
	public class TeamOutOfAdherenceReadModelUpdater : IHandleEvent<PersonInAdherenceEvent>, IHandleEvent<PersonOutOfAdherenceEvent>
	{
		private readonly ITeamOutOfAdherenceReadModelPersister _readModelPersister;

		public TeamOutOfAdherenceReadModelUpdater(ITeamOutOfAdherenceReadModelPersister readModelPersister)
		{
			_readModelPersister = readModelPersister;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			var current = _readModelPersister.Get(@event.TeamId);
			if (current != null)
			{
				if (!current.PersonIds.Contains(@event.PersonId.ToString())) return;

				current.Count--;
				current.PersonIds = current.PersonIds.Replace(@event.PersonId.ToString(), "");
				_readModelPersister.Persist(current);
			}
			else
			{
				_readModelPersister.Persist(new TeamOutOfAdherenceReadModel() { TeamId = @event.TeamId, SiteId = @event.SiteId });
			}
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			var current = _readModelPersister.Get(@event.TeamId);
			if (current != null)
			{
				current.Count++;
				current.PersonIds += @event.PersonId.ToString();
				_readModelPersister.Persist(current);
			}
			else
			{
				_readModelPersister.Persist(new TeamOutOfAdherenceReadModel() { TeamId = @event.TeamId, Count = 1, SiteId = @event.SiteId });
			}
		}

	}
}