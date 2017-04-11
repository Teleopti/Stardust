using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class HistoricalAdherenceUpdater : 
		IRunOnHangfire,
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>,
		IHandleEvent<PersonStateChangedEvent>,
		IHandleEvent<PersonRuleChangedEvent>,
		IHandleEvent<TenantDayTickEvent>
	{
		private readonly IHistoricalAdherenceReadModelPersister _adherencePersister;
		private readonly IHistoricalChangeReadModelPersister _historicalChangePersister;
		private readonly INow _now;

		public HistoricalAdherenceUpdater(
			IHistoricalAdherenceReadModelPersister adherencePersister, 
			IHistoricalChangeReadModelPersister historicalChangePersister, 
			INow now)
		{
			_adherencePersister = adherencePersister;
			_now = now;
			_historicalChangePersister = historicalChangePersister;
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			_adherencePersister.AddOut(@event.PersonId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			_adherencePersister.AddIn(@event.PersonId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			_adherencePersister.AddNeutral(@event.PersonId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SolidProofWhenManagingAgentAdherence_39351)]
		public virtual void Handle(PersonStateChangedEvent @event)
		{
			_historicalChangePersister.Persist(new HistoricalChangeReadModel
			{
				PersonId = @event.PersonId,
				BelongsToDate = @event.BelongsToDate,
				Timestamp = @event.Timestamp,
				StateName = @event.StateName,
				StateGroupId = @event.StateGroupId,
				ActivityName = @event.ActivityName,
				ActivityColor = @event.ActivityColor,
				RuleName = @event.RuleName,
				RuleColor = @event.RuleColor,
				Adherence = adherenceFor(@event.Adherence)
			});
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SolidProofWhenManagingAgentAdherence_39351)]
		public virtual void Handle(PersonRuleChangedEvent @event)
		{
			_historicalChangePersister.Persist(new HistoricalChangeReadModel
			{
				PersonId = @event.PersonId,
				BelongsToDate = @event.BelongsToDate,
				Timestamp = @event.Timestamp,
				StateName = @event.StateName,
				StateGroupId = @event.StateGroupId,
				ActivityName = @event.ActivityName,
				ActivityColor = @event.ActivityColor,
				RuleName = @event.RuleName,
				RuleColor = @event.RuleColor,
				Adherence = adherenceFor(@event.Adherence)
			});
		}

		private static HistoricalChangeInternalAdherence? adherenceFor(EventAdherence? eventAdherence)
		{
			if (!eventAdherence.HasValue)
				return null;

			switch (eventAdherence.Value)
			{
				case EventAdherence.In:
					return HistoricalChangeInternalAdherence.In;
				case EventAdherence.Out:
					return HistoricalChangeInternalAdherence.Out;
				case EventAdherence.Neutral:
					return HistoricalChangeInternalAdherence.Neutral;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(TenantDayTickEvent tenantDayTickEvent)
		{
			_adherencePersister.Remove(_now.UtcDateTime().Date.AddDays(-5));
		}
	}

}
