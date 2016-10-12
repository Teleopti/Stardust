using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class HistoricalAdherenceReadModelMaintainer : 
		IRunOnHangfire,
		IHandleEvent<PersonAssociationChangedEvent>,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<ScheduleChangedEvent>
	{
		private readonly IHistoricalAdherenceReadModelPersister _persister;

		public HistoricalAdherenceReadModelMaintainer(IHistoricalAdherenceReadModelPersister persister)
		{
			_persister = persister;
		}

		[UnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonAssociationChangedEvent @event)

		{
			_persister.Persist(new HistoricalAdherenceReadModel
			{
				PersonId = @event.PersonId
			});
		}

		[UnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			_persister.SetInAdherence(@event.PersonId);
		}

		[UnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			_persister.SetNeutralAdherence(@event.PersonId);
		}

		[UnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			_persister.SetOutOfAdherence(@event.PersonId);
		}

		[UnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			_persister.UpdateSchedule(@event.PersonId);
		}
	}

	public class AgentStateReadModelMaintainer :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>
	{
		private readonly IAgentStateReadModelPersister _persister;
		private readonly INow _now;

		public AgentStateReadModelMaintainer(IAgentStateReadModelPersister persister, INow now)
		{
			_persister = persister;
			_now = now;
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.SetDeleted(@event.PersonId, expirationFor(@event));
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			_persister.DeleteOldRows(_now.UtcDateTime());

			if (@event.TeamId.HasValue)
			{
				var model = _persister.Get(@event.PersonId);
				if (model == null || expirationFor(@event) >= model.ExpiresAt.GetValueOrDefault())
					_persister.UpsertAssociation(@event.PersonId, @event.TeamId.Value, @event.SiteId, @event.BusinessUnitId);
			}
			
			else
				_persister.SetDeleted(@event.PersonId, expirationFor(@event));
		}

		private static DateTime expirationFor(IEvent @event)
		{
			return ((dynamic) @event).Timestamp.AddMinutes(30);
		}
	}
}