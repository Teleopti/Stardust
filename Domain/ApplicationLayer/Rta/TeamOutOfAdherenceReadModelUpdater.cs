using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	[UseOnToggle(Toggles.RTA_NoBroker_31237)]
	public class TeamOutOfAdherenceReadModelUpdater : 
		IHandleEvent<PersonInAdherenceEvent>, 
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IInitializeble,
		IRecreatable
	{
		private readonly ITeamOutOfAdherenceReadModelPersister _persister;

		public TeamOutOfAdherenceReadModelUpdater(ITeamOutOfAdherenceReadModelPersister persister)
		{
			_persister = persister;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			var model = _persister.Get(@event.TeamId) ??
				new TeamOutOfAdherenceReadModel() { TeamId = @event.TeamId, SiteId = @event.SiteId };
			model.PersonIds += @event.PersonId;
			model.Count++;
			_persister.Persist(model);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			var model = _persister.Get(@event.TeamId);
			if (model == null)
			{
				_persister.Persist(new TeamOutOfAdherenceReadModel() { TeamId = @event.TeamId, SiteId = @event.SiteId });
				return;
			}
			if (!model.PersonIds.Contains(@event.PersonId.ToString()))
				return;
			model.PersonIds = model.PersonIds.Replace(@event.PersonId.ToString(), "");
			model.Count--;
			_persister.Persist(model);
		}

		public bool Initialized()
		{
			return _persister.HasData();
		}

		public void DeleteAll()
		{
			_persister.Clear();
		}
	}
}