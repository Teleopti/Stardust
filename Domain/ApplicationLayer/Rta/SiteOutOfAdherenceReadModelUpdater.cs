using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	[UseOnToggle(Toggles.RTA_NoBroker_31237)]
	public class SiteOutOfAdherenceReadModelUpdater : 
		IHandleEvent<PersonOutOfAdherenceEvent>, 
		IHandleEvent<PersonInAdherenceEvent>,
		IInitializeble,
		IRecreatable
	{
		private readonly ISiteOutOfAdherenceReadModelPersister _persister;

		public SiteOutOfAdherenceReadModelUpdater(ISiteOutOfAdherenceReadModelPersister persister)
		{
			_persister = persister;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(@event.BusinessUnitId, @event.SiteId, model =>
				updatePerson(@event.PersonId, model, person =>
				{
					person.OutOfAdherence += 1;
				}));
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(@event.BusinessUnitId, @event.SiteId, model =>
				updatePerson(@event.PersonId, model, person =>
				{
					person.OutOfAdherence -= 1;
				}));
		}

		private void handleEvent(Guid businessUnitId, Guid siteId, Action<SiteOutOfAdherenceReadModel> mutate)
		{
			var model = _persister.Get(siteId) ?? new SiteOutOfAdherenceReadModel
			{
				SiteId = siteId,
				BusinessUnitId = businessUnitId,
				State = new SiteOutOfAdherenceReadModelState[] { }
			};
			mutate(model);
			calculate(model);
			_persister.Persist(model);
		}

		private void updatePerson(Guid personId, SiteOutOfAdherenceReadModel model, Action<SiteOutOfAdherenceReadModelState> mutate)
		{
			var person = getPerson(model, personId);
			mutate(person);
			if (person.OutOfAdherence == 0)
				removePerson(model, person);
		}

		private static SiteOutOfAdherenceReadModelState getPerson(SiteOutOfAdherenceReadModel model, Guid personId)
		{
			var person = model.State.SingleOrDefault(x => x.PersonId == personId);
			if (person != null) return person;
			person = new SiteOutOfAdherenceReadModelState {PersonId = personId};
			model.State = model.State.Concat(new[] {person}).ToArray();
			return person;
		}

		private void removePerson(SiteOutOfAdherenceReadModel model, SiteOutOfAdherenceReadModelState person)
		{
			model.State = model.State.Except(new[] {person}).ToArray();
		}

		private static void calculate(SiteOutOfAdherenceReadModel model)
		{
			model.Count = model.State.Count(x => x.OutOfAdherence > 0);
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