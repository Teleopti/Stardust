using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	[UseOnToggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	public class SiteOutOfAdherenceReadModelUpdater : 
		IRunOnHangfire,
		IHandleEvent<PersonOutOfAdherenceEvent>, 
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>,
		IHandleEvent<PersonDeletedEvent>,
		IInitializeble,
		IRecreatable
	{
		private readonly ISiteOutOfAdherenceReadModelPersister _persister;
		private readonly ISiteOutOfAdherenceReadModelReader _reader;

		public SiteOutOfAdherenceReadModelUpdater(
			ISiteOutOfAdherenceReadModelPersister persister, 
			ISiteOutOfAdherenceReadModelReader reader)
		{
			_persister = persister;
			_reader = reader;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(@event.BusinessUnitId, @event.SiteId, model =>
				updatePerson(model, @event.PersonId, @event.Timestamp, person =>
				{
					person.OutOfAdherence = true;
				}));
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(@event.BusinessUnitId, @event.SiteId, model =>
				updatePerson(model, @event.PersonId, @event.Timestamp, person =>
				{
					person.OutOfAdherence = false;
				}));
		}
		
		[ReadModelUnitOfWork]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			handleEvent(@event.BusinessUnitId, @event.SiteId, model =>
				updatePerson(model, @event.PersonId, @event.Timestamp, person =>
				{
					person.OutOfAdherence = false;
				}));
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			var models = 
				from model in _reader.Read()
				from state in model.State
				where state.PersonId == @event.PersonId
				select model;
			
			foreach (var model in models)
			{
				deletePerson(model, @event.PersonId);
				calculate(model);
				_persister.Persist(model);
			}
		}

		private static void deletePerson(SiteOutOfAdherenceReadModel model, Guid personId)
		{
			var state = stateForPerson(model, personId);
			state.Deleted = true;
			state.OutOfAdherence = false;
			removeRedundantOldStates(model);
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

		private static void updatePerson(SiteOutOfAdherenceReadModel model, Guid personId, DateTime time, Action<SiteOutOfAdherenceReadModelState> mutate)
		{
			var state = stateForPerson(model, personId);
			if (state.Time <= time && !state.Deleted)
			{
				state.Time = time;
				mutate(state);
			}
			removeRedundantOldStates(model);
		}

		private static void removeRedundantOldStates(SiteOutOfAdherenceReadModel model)
		{
			var latestUpdate = model.State.Max(x => x.Time);
			if (latestUpdate == DateTime.MinValue)
				return;
			var safeToRemoveOlderThan = latestUpdate.Subtract(TimeSpan.FromMinutes(10));
			model.State = model.State
				.Where(x => x.OutOfAdherence || x.Time > safeToRemoveOlderThan)
				.ToArray();
		}

		private static SiteOutOfAdherenceReadModelState stateForPerson(SiteOutOfAdherenceReadModel model, Guid personId)
		{
			var person = model.State.SingleOrDefault(x => x.PersonId == personId);
			if (person != null) return person;
			person = new SiteOutOfAdherenceReadModelState { PersonId = personId };
			model.State = model.State.Concat(new[] { person }).ToArray();
			return person;
		}

		private static void calculate(SiteOutOfAdherenceReadModel model)
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