using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	[UseOnToggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	public class TeamOutOfAdherenceReadModelUpdater : 
		IRunOnHangfire,
		IHandleEvent<PersonInAdherenceEvent>, 
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>,
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
			updateModel(
				@event.PersonId,
				@event.Timestamp,
				@event.SiteId,
				@event.TeamId,
				outPerson
				);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			updateModel(
				@event.PersonId,
				@event.Timestamp,
				@event.SiteId,
				@event.TeamId,
				notOutPerson
				);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			updateModel(
				@event.PersonId,
				@event.Timestamp,
				@event.SiteId,
				@event.TeamId,
				notOutPerson
				);
		}

		[UseOnToggle(Toggles.RTA_DeletedPersons_36041)]
		[ReadModelUnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			updateAllModels(
				@event.PersonId,
				@event.Timestamp,
				deletePerson);
		}

		[UseOnToggle(Toggles.RTA_TerminatedPersons_36042)]
		[ReadModelUnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (@event.TeamId != null)
			{
				updateAllModels(
					@event.PersonId,
					@event.Timestamp,
					(m, p, t) =>
						m.TeamId == @event.TeamId
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

		private void updateModel(Guid personId, DateTime time, Guid siteId, Guid teamId, UpdateAction update)
		{
			var model = modelFor(siteId, teamId);
			updateModel(model, personId, time, update);
		}
		
		private void updateAllModels(Guid personId, DateTime time, UpdateAction update)
		{
			_persister.GetAll()
				.ForEach(m => updateModel(m, personId, time, update));
		}

		private void updateModel(TeamOutOfAdherenceReadModel model, Guid personId, DateTime time, UpdateAction update)
		{
			if (!update(model, personId, time))
				return;
			removeRedundantOldStates(model);
			calculate(model);
			_persister.Persist(model);
		}

		private TeamOutOfAdherenceReadModel modelFor(Guid siteId, Guid teamId)
		{
			return _persister.Get(teamId) ?? new TeamOutOfAdherenceReadModel
			{
				SiteId = siteId,
				TeamId = teamId,
				State = new TeamOutOfAdherenceReadModelState[] { }
			};
		}

		private delegate bool UpdateAction(TeamOutOfAdherenceReadModel model, Guid personId, DateTime time);

		private static bool deletePerson(TeamOutOfAdherenceReadModel model, Guid personId, DateTime time)
		{
			var state = stateForPerson(model, personId);
			state.Time = time;
			state.Deleted = true;
			return true;
		}

		private static bool movePersonFrom(TeamOutOfAdherenceReadModel model, Guid personId, DateTime time)
		{
			var state = stateForPerson(model, personId);
			state.Time = state.Time > time ? state.Time : time;
			state.Moved = true;
			return true;
		}

		private static bool movePersonTo(TeamOutOfAdherenceReadModel model, Guid personId, DateTime time)
		{
			var state = stateForPerson(model, personId);
			state.Time = state.Time > time ? state.Time : time;
			state.Moved = false;
			return true;
		}

		private static bool notOutPerson(TeamOutOfAdherenceReadModel model, Guid personId, DateTime time)
		{
			var state = stateForPerson(model, personId);
			if (state.Time > time)
				return false;
			state.Time = time;
			state.OutOfAdherence = false;
			return true;
		}

		private static bool outPerson(TeamOutOfAdherenceReadModel model, Guid personId, DateTime time)
		{
			var state = stateForPerson(model, personId);
			if (state.Time > time)
				return false;
			state.Time = time;
			state.OutOfAdherence = true;
			return true;
		}
		
		private static void removeRedundantOldStates(TeamOutOfAdherenceReadModel model)
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