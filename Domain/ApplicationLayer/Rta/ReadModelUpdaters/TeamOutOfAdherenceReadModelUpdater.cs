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
		IInitializeble,
		IRecreatable
	{
		private readonly ITeamOutOfAdherenceReadModelPersister _persister;
		private readonly ITeamOutOfAdherenceReadModelReader _reader;

		public TeamOutOfAdherenceReadModelUpdater(
			ITeamOutOfAdherenceReadModelPersister persister, 
			ITeamOutOfAdherenceReadModelReader reader)
		{
			_persister = persister;
			_reader = reader;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(@event.SiteId, @event.TeamId, model =>
				updatePerson(model, @event.PersonId, @event.Timestamp, person =>
				{
					person.OutOfAdherence = true;
				}));
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(@event.SiteId, @event.TeamId, model =>
				updatePerson(model, @event.PersonId, @event.Timestamp, person =>
				{
					person.OutOfAdherence = false;
				}));
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			handleEvent(@event.SiteId, @event.TeamId, model =>
				updatePerson(model, @event.PersonId, @event.Timestamp, person =>
				{
					person.OutOfAdherence = false;
				}));
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			if (@event.PersonPeriodsBefore == null || @event.PersonPeriodsBefore.IsEmpty())
				return;

			foreach (var personPeriod in @event.PersonPeriodsBefore)
			{
				handleEvent(personPeriod.SiteId, personPeriod.TeamId, model =>
					deletePerson(model, @event.PersonId, @event.Timestamp, person =>
					{
						person.Deleted = true;
						person.OutOfAdherence = false;
					}));
			}
		}
		
		private void handleEvent(Guid siteId, Guid teamId, Action<TeamOutOfAdherenceReadModel> mutate)
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

		private static void deletePerson(TeamOutOfAdherenceReadModel model, Guid personId, DateTime time, Action<TeamOutOfAdherenceReadModelState> mutate)
		{
			var state = stateForPerson(model, personId);
			state.Time = time;
			mutate(state);
			removeRedundantOldStates(model);
		}

		private static void updatePerson(TeamOutOfAdherenceReadModel model, Guid personId, DateTime time, Action<TeamOutOfAdherenceReadModelState> mutate)
		{
			var state = stateForPerson(model, personId);
			if (state.Time <= time && !state.Deleted)
			{
				state.Time = time;
				mutate(state);
			}
			removeRedundantOldStates(model);
		}

		private static void removeRedundantOldStates(TeamOutOfAdherenceReadModel model)
		{
			var latestUpdate = model.State.Max(x => x.Time);
			if (latestUpdate == DateTime.MinValue)
				return;
			var safeToRemoveOlderThan = latestUpdate.Subtract(TimeSpan.FromMinutes(10));
			model.State = model.State
				.Where(x => x.OutOfAdherence || x.Time > safeToRemoveOlderThan)
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