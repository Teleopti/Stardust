using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	[DisabledBy(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
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

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			updateAllModels(
				@event.PersonId,
				@event.Timestamp,
				deletePerson);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (@event.Version == 2)
			{
				if (!@event.PreviousAssociation.IsNullOrEmpty())
					@event.PreviousAssociation.ForEach(
						a =>
						{
							var model = _persister.Get(a.SiteId);
							if (model != null)
								updateModel(model, @event.PersonId, @event.Timestamp, movePersonFrom);
						});
				if (@event.SiteId != null)
				{
					var model = _persister.Get(@event.SiteId.Value);
					if (model != null)
						updateModel(model, @event.PersonId, @event.Timestamp, movePersonTo);
				}
			}
			else
			{
				if (@event.SiteId != null)
				{
					updateAllModels(
						@event.PersonId,
						@event.Timestamp,
						(m, p, t) =>
							m.SiteId == @event.SiteId
								? movePersonTo(m, p, t)
								: movePersonFrom(m, p, t)
						);
				}
				else
				{
					updateAllModels(
						@event.PersonId,
						@event.Timestamp,
						movePersonFrom);
				}
			}
		}

		private void updateModel(Guid personId, DateTime time, Guid businessUnitId, Guid siteId, UpdateAction update)
		{
			var model = modelFor(businessUnitId, siteId);
			updateModel(model, personId, time, update);
		}
		
		private void updateAllModels(Guid personId, DateTime time, UpdateAction update)
		{
			_persister.GetAll()
				.ForEach(m => updateModel(m, personId, time, update));
		}

		private void updateModel(SiteOutOfAdherenceReadModel model, Guid personId, DateTime time, UpdateAction update)
		{
			if (!update(model, personId, time))
				return;
			removeRedundantOldStates(model);
			calculate(model);
			_persister.Persist(model);
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

		private static bool movePersonFrom(SiteOutOfAdherenceReadModel model, Guid personId, DateTime time)
		{
			var state = stateForPerson(model, personId);
			state.Time = state.Time > time ? state.Time : time;
			state.Moved = true;
			return true;
		}

		private static bool movePersonTo(SiteOutOfAdherenceReadModel model, Guid personId, DateTime time)
		{
			var state = stateForPerson(model, personId);
			state.Time = state.Time > time ? state.Time : time;
			state.Moved = false;
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
			model.State = model.State
				.Except(x =>
				{
					var bufferTimeOut = x.Time < latestUpdate.Subtract(TimeSpan.FromMinutes(10));
					if (x.Moved || x.Deleted)
						return bufferTimeOut;
					if (x.OutOfAdherence)
						return false;
					return bufferTimeOut;
				})
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
					!x.Deleted && 
					!x.Moved);
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