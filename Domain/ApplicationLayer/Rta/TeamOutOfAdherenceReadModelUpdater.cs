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
			handleEvent(@event.BusinessUnitId, @event.SiteId, model =>
				updatePerson(@event.PersonId, model, person =>
				{
					person.Count += 1;
				}));
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(@event.BusinessUnitId, @event.SiteId, model =>
				updatePerson(@event.PersonId, model, person =>
				{
					person.Count -= 1;
				}));
		}

		private void handleEvent(Guid teamId, Guid siteId, Action<TeamOutOfAdherenceReadModel> mutate)
		{
			var model = _persister.Get(siteId) ?? new TeamOutOfAdherenceReadModel
			{
				SiteId = siteId,
				TeamId = teamId,
				State = new TeamOutOfAdherenceReadModelState[] { }
			};
			mutate(model);
			calculate(model);
			_persister.Persist(model);
		}

		private void updatePerson(Guid personId, TeamOutOfAdherenceReadModel model, Action<TeamOutOfAdherenceReadModelState> mutate)
		{
			var person = getPerson(model, personId);
			mutate(person);
			if (person.Count == 0)
				removePerson(model, person);
		}

		private static TeamOutOfAdherenceReadModelState getPerson(TeamOutOfAdherenceReadModel model, Guid personId)
		{
			var person = model.State.SingleOrDefault(x => x.PersonId == personId);
			if (person != null) return person;
			person = new TeamOutOfAdherenceReadModelState { PersonId = personId };
			model.State = model.State.Concat(new[] { person }).ToArray();
			return person;
		}

		private void removePerson(TeamOutOfAdherenceReadModel model, TeamOutOfAdherenceReadModelState person)
		{
			model.State = model.State.Except(new[] { person }).ToArray();
		}

		private static void calculate(TeamOutOfAdherenceReadModel model)
		{
			model.Count = model.State.Count(x => x.Count > 0);
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