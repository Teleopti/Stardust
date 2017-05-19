using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	[EnabledBy(Toggles.RTA_AsyncOptimization_43924)]
	public class HistoricalAdherenceUpdaterInSync : HistoricalAdherenceUpdaterImpl,
		IHandleEvents,
		IRunInSync
	{
		public HistoricalAdherenceUpdaterInSync(IHistoricalAdherenceReadModelPersister adherencePersister,
			IHistoricalChangeReadModelPersister historicalChangePersister) : base(adherencePersister, historicalChangePersister)
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
		public virtual void Handle(IEnumerable<IEvent> events)
		{
			events.ForEach(e => handle((dynamic) e));
		}
	}

	[EnabledBy(Toggles.RTA_EventPackagesOptimization_43924)]
	[DisabledBy(Toggles.RTA_AsyncOptimization_43924)]
	public class HistoricalAdherenceUpdaterWithPackages : HistoricalAdherenceUpdaterImpl, 
		IHandleEvents,
		IRunOnHangfire
	{
		public HistoricalAdherenceUpdaterWithPackages(IHistoricalAdherenceReadModelPersister adherencePersister, IHistoricalChangeReadModelPersister historicalChangePersister) : base(adherencePersister, historicalChangePersister)
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
		public virtual void Handle(IEnumerable<IEvent> events)
		{
			events.ForEach(e => handle((dynamic)e));
		}
	}

	[DisabledBy(Toggles.RTA_EventPackagesOptimization_43924)]
	public class HistoricalAdherenceUpdater : HistoricalAdherenceUpdaterImpl, 
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>,
		IHandleEvent<PersonStateChangedEvent>,
		IHandleEvent<PersonRuleChangedEvent>,
		IRunOnHangfire
	{
		public HistoricalAdherenceUpdater(IHistoricalAdherenceReadModelPersister adherencePersister, IHistoricalChangeReadModelPersister historicalChangePersister)
			: base(adherencePersister, historicalChangePersister)
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

		protected HistoricalAdherenceUpdaterImpl(
			IHistoricalAdherenceReadModelPersister adherencePersister, 
			IHistoricalChangeReadModelPersister historicalChangePersister)
		{
			_adherencePersister = adherencePersister;
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

	}

}
