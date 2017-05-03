using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	[EnabledBy(Toggles.RTA_NoHangfireExperiment_43924)]
	public class HistoricalAdherenceUpdaterInSync : HistoricalAdherenceUpdaterImpl, IRunInSync
	{
		public HistoricalAdherenceUpdaterInSync(IHistoricalAdherenceReadModelPersister adherencePersister, IHistoricalChangeReadModelPersister historicalChangePersister, INow now) : base(adherencePersister, historicalChangePersister, now)
		{
		}
	}

	[DisabledBy(Toggles.RTA_NoHangfireExperiment_43924)]
	public class HistoricalAdherenceUpdater : HistoricalAdherenceUpdaterImpl, IRunOnHangfire
	{
		public HistoricalAdherenceUpdater(IHistoricalAdherenceReadModelPersister adherencePersister,
			IHistoricalChangeReadModelPersister historicalChangePersister, INow now)
			: base(adherencePersister, historicalChangePersister, now)
		{
		}
	}

	[EnabledBy(Toggles.HangFire_EventPackages_43924)]
	public class HistoricalAdherenceUpdaterWithPackages : HistoricalAdherenceUpdater, IHandleEvents
	{
		public HistoricalAdherenceUpdaterWithPackages(IHistoricalAdherenceReadModelPersister adherencePersister, IHistoricalChangeReadModelPersister historicalChangePersister, INow now) : base(adherencePersister, historicalChangePersister, now)
		{
		}

		public void Subscribe(ISubscriptionsRegistrator subscriptionsRegistrator)
		{
			subscriptionsRegistrator.Add<PersonOutOfAdherenceEvent>();
			subscriptionsRegistrator.Add<PersonInAdherenceEvent>();
			subscriptionsRegistrator.Add<PersonNeutralAdherenceEvent>();
			subscriptionsRegistrator.Add<PersonStateChangedEvent>();
			subscriptionsRegistrator.Add<PersonRuleChangedEvent>();
		}

		[ReadModelUnitOfWork]
		public void Handle(IEnumerable<IEvent> events)
		{
			foreach (var @event in events)
			{
				if (@event is PersonOutOfAdherenceEvent)
					handle(@event as PersonOutOfAdherenceEvent);
				if (@event is PersonInAdherenceEvent)
					handle(@event as PersonInAdherenceEvent);
				if (@event is PersonRuleChangedEvent)
					handle(@event as PersonRuleChangedEvent);
				if (@event is PersonStateChangedEvent)
					handle(@event as PersonStateChangedEvent);
				if (@event is PersonNeutralAdherenceEvent)
					handle(@event as PersonNeutralAdherenceEvent);
			}
		}
	}

	public abstract class HistoricalAdherenceUpdaterImpl : 
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

		protected HistoricalAdherenceUpdaterImpl(
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
			handle(@event);
		}

		protected void handle(PersonOutOfAdherenceEvent @event)
		{
			_adherencePersister.AddOut(@event.PersonId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handle(@event);
		}

		protected void handle(PersonInAdherenceEvent @event)
		{
			_adherencePersister.AddIn(@event.PersonId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			handle(@event);
		}

		protected void handle(PersonNeutralAdherenceEvent @event)
		{
			_adherencePersister.AddNeutral(@event.PersonId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SolidProofWhenManagingAgentAdherence_39351)]
		public virtual void Handle(PersonStateChangedEvent @event)
		{
			handle(@event);
		}

		protected void handle(PersonStateChangedEvent @event)
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
			handle(@event);
		}

		protected void handle(PersonRuleChangedEvent @event)
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
