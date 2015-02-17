using System;
using System.Collections.Generic;
using System.Linq;
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
			handleModel(@event.TeamId, @event.SiteId,  model =>
			{
				model.State = updateStates(model.State, @event.PersonId, 1);
			});
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleModel(@event.TeamId, @event.SiteId, model =>
			{
				model.State = updateStates(model.State, @event.PersonId, -1);
			});
		}

		private void handleModel(Guid teamId, Guid siteId, Action<TeamOutOfAdherenceReadModel> mutate)
		{
			var model = _persister.Get(teamId) ?? new TeamOutOfAdherenceReadModel()
			{
				SiteId = siteId,
				TeamId = teamId,
				State = new TeamOutOfAdherenceReadModelState[] { }
			};
			mutate(model);
			model.Count = model.State.Count(x=>x.AdherenceCounter>0);
			_persister.Persist(model);
		}

		private IEnumerable<TeamOutOfAdherenceReadModelState> updateStates( IEnumerable<TeamOutOfAdherenceReadModelState> states, Guid personId, int adherenceState)
		{
			if (!states.Any(x => x.PersonId == personId))
				return states.Concat(new[] {new TeamOutOfAdherenceReadModelState() {AdherenceCounter = adherenceState, PersonId = personId}});

			if (states.Any(x => x.PersonId == personId && ((x.AdherenceCounter + adherenceState) <= 0)))
				return states.Where(x => x.PersonId != personId);

			foreach (var state in states.Where(state => state.PersonId == personId))
			{
				state.AdherenceCounter += adherenceState;
			}
			return states;
		}
		
		[ReadModelUnitOfWork]
		public virtual bool Initialized()
		{
			return _persister.HasData();
		}

		[ReadModelUnitOfWork]
		public virtual void DeleteAll()
		{
			_persister.Clear();
		}
	}
}