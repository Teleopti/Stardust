using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
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
		IHandleEvent<PersonAssociationChangedEvent>,
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
			updateModel(
				@event.PersonId,
				@event.Timestamp,
				@event.BusinessUnitId,
				@event.SiteId, 
				outPerson);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			updateModel(
				@event.PersonId,
				@event.Timestamp,
				@event.BusinessUnitId,
				@event.SiteId, 
				notOutPerson);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			updateModel(
				@event.PersonId,
				@event.Timestamp,
				@event.BusinessUnitId,
				@event.SiteId, 
				notOutPerson);
		}

		[UseOnToggle(Toggles.RTA_DeletedPersons_36041)]
		[ReadModelUnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			var sites = @event.PersonPeriodsBefore.EmptyIfNull()
				.Select(p => new site { SiteId = p.SiteId, BusinessUnitId = p.BusinessUnitId});
			updateAllModels(
				@event.PersonId,
				@event.Timestamp,
				sites,
				deletePerson);
		}

		[UseOnToggle(Toggles.RTA_TerminatedPersons_36042)]
		[ReadModelUnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			updateAllModels(
				@event.PersonId,
				@event.Timestamp,
				null,
				deletePerson);
		}

		private void updateModel(Guid personId, DateTime time, Guid businessUnitId, Guid siteId, UpdateAction update)
		{
			var model = modelFor(businessUnitId, siteId);
			updateModel(model, personId, time, update);
		}

		private void updateAllModels(Guid personId, DateTime time, IEnumerable<site> sites, UpdateAction update)
		{
			var existingModels = _persister.GetAll();

			var newModels =
				from t in sites.EmptyIfNull()
				let unknown = !existingModels.Select(m => m.SiteId).Contains(t.SiteId)
				where unknown
				select modelFor(t.BusinessUnitId, t.SiteId);

			var models = existingModels.Concat(newModels);

			models.ForEach(m =>
			{
				updateModel(m, personId, time, update);
			});
		}

		private void updateModel(SiteOutOfAdherenceReadModel model, Guid personId, DateTime time, UpdateAction update)
		{
			if (!update(model, personId, time))
				return;
			removeRedundantOldStates(model);
			calculate(model);
			_persister.Persist(model);
		}

		private class site
		{
			public Guid BusinessUnitId;
			public Guid SiteId;
		}

		private SiteOutOfAdherenceReadModel modelFor(Guid businessUnitId, Guid siteId)
		{
			return _persister.Get(siteId) ?? new SiteOutOfAdherenceReadModel
			{
				SiteId = siteId,
				BusinessUnitId = businessUnitId,
				State = new SiteOutOfAdherenceReadModelState[] { }
			};
		}

		private delegate bool UpdateAction(SiteOutOfAdherenceReadModel model, Guid personId, DateTime time);

		private static bool deletePerson(SiteOutOfAdherenceReadModel model, Guid personId, DateTime time)
		{
			var state = stateForPerson(model, personId);
			state.Time = time;
			state.Deleted = true;
			return true;
		}

		private static bool notOutPerson(SiteOutOfAdherenceReadModel model, Guid personId, DateTime time)
		{
			var state = stateForPerson(model, personId);
			if (state.Time > time)
				return false;
			state.Time = time;
			state.OutOfAdherence = false;
			return true;
		}

		private static bool outPerson(SiteOutOfAdherenceReadModel model, Guid personId, DateTime time)
		{
			var state = stateForPerson(model, personId);
			if (state.Time > time)
				return false;
			state.Time = time;
			state.OutOfAdherence = true;
			return true;
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
			model.Count = model.State
				.Count(x =>
					x.OutOfAdherence &&
					!x.Deleted);
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