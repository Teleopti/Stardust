﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	[EnabledBy(Toggles.RTA_EventPackagesExperiment_43924)]
	public class HistoricalAdherenceUpdaterWithPackages : HistoricalAdherenceUpdaterImpl, 
		IHandleEvents,
		IHandleEvent<TenantDayTickEvent>,
		IRunOnHangfire
	{
		public HistoricalAdherenceUpdaterWithPackages(IHistoricalAdherenceReadModelPersister adherencePersister, IHistoricalChangeReadModelPersister historicalChangePersister, INow now) :
			base(adherencePersister, historicalChangePersister, now)
		{
		}

		public void Subscribe(SubscriptionRegistrator registrator)
		{
			registrator.SubscribeTo<PersonOutOfAdherenceEvent>();
			registrator.SubscribeTo<PersonInAdherenceEvent>();
			registrator.SubscribeTo<PersonNeutralAdherenceEvent>();
			registrator.SubscribeTo<PersonStateChangedEvent>();
			registrator.SubscribeTo<PersonRuleChangedEvent>();
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

	[DisabledBy(Toggles.RTA_EventPackagesExperiment_43924)]
	public class HistoricalAdherenceUpdater : HistoricalAdherenceUpdaterImpl, 
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>,
		IHandleEvent<PersonStateChangedEvent>,
		IHandleEvent<PersonRuleChangedEvent>,
		IHandleEvent<TenantDayTickEvent>,
		IRunOnHangfire
	{
		public HistoricalAdherenceUpdater(IHistoricalAdherenceReadModelPersister adherencePersister, IHistoricalChangeReadModelPersister historicalChangePersister, INow now)
			: base(adherencePersister, historicalChangePersister, now)
		{
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handle(@event);
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handle(@event);
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			handle(@event);
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SolidProofWhenManagingAgentAdherence_39351)]
		public virtual void Handle(PersonStateChangedEvent @event)
		{
			handle(@event);
		}

		[ReadModelUnitOfWork]
		[EnabledBy(Toggles.RTA_SolidProofWhenManagingAgentAdherence_39351)]
		public virtual void Handle(PersonRuleChangedEvent @event)
		{
			handle(@event);
		}
	}

	public abstract class HistoricalAdherenceUpdaterImpl
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

		protected void handle(PersonOutOfAdherenceEvent @event)
		{
			_adherencePersister.AddOut(@event.PersonId, @event.Timestamp);
		}

		protected void handle(PersonInAdherenceEvent @event)
		{
			_adherencePersister.AddIn(@event.PersonId, @event.Timestamp);
		}

		protected void handle(PersonNeutralAdherenceEvent @event)
		{
			_adherencePersister.AddNeutral(@event.PersonId, @event.Timestamp);
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
