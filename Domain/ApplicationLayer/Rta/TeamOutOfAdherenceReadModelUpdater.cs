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
			handleEvent(@event.TeamId, @event.SiteId, model =>
				updatePerson(model, @event.PersonId, @event.Timestamp, person =>
				{
					person.OutOfAdherence = true;
				}));
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(@event.TeamId, @event.SiteId, model =>
				updatePerson(model, @event.PersonId, @event.Timestamp, person =>
				{
					person.OutOfAdherence = false;
				}));
		}

		private void handleEvent(Guid teamId, Guid siteId, Action<TeamOutOfAdherenceReadModel> mutate)
		{
			var model = _persister.Get(teamId) ?? new TeamOutOfAdherenceReadModel
			{
				SiteId = siteId,
				TeamId = teamId,
				State = new TeamOutOfAdherenceReadModelState[] { }
			};
			mutate(model);
			calculate(model);
			_persister.Persist(model);
		}

		private void updatePerson(TeamOutOfAdherenceReadModel model, Guid personId, DateTime time, Action<TeamOutOfAdherenceReadModelState> mutate)
		{
			var state = stateForPerson(model, personId);
			if (state.Time <= time)
			{
				state.Time = time;
				mutate(state);	
			}
		}

		private static TeamOutOfAdherenceReadModelState stateForPerson(TeamOutOfAdherenceReadModel model, Guid personId)
		{
			var person = model.State.SingleOrDefault(x => x.PersonId == personId);
			if (person != null) return person;
			person = new TeamOutOfAdherenceReadModelState { PersonId = personId };
			model.State = model.State.Concat(new[] { person }).ToArray();
			return person;
		}

		private static void calculate(TeamOutOfAdherenceReadModel model)
		{
			model.Count = model.State.Count(x => x.OutOfAdherence);
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