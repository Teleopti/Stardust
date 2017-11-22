using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public static class HistoricalAdherenceUpdaterExtensions
	{
		public static void Handle(this HistoricalAdherenceUpdater instance, PersonOutOfAdherenceEvent @event)
		{
			instance.Handle(@event.AsArray());
		}

		public static void Handle(this HistoricalAdherenceUpdater instance, PersonInAdherenceEvent @event)
		{
			instance.Handle(@event.AsArray());
		}

		public static void Handle(this HistoricalAdherenceUpdater instance, PersonNeutralAdherenceEvent @event)
		{
			instance.Handle(@event.AsArray());
		}
	}

	public class HistoricalAdherenceUpdater :
		IHandleEvents,
		IRunInSync
	{
		private readonly IHistoricalAdherenceReadModelPersister _adherencePersister;
		private readonly IHistoricalChangeReadModelPersister _historicalChangePersister;

		public HistoricalAdherenceUpdater(
			IHistoricalAdherenceReadModelPersister adherencePersister,
			IHistoricalChangeReadModelPersister historicalChangePersister)
		{
			_adherencePersister = adherencePersister;
			_historicalChangePersister = historicalChangePersister;
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

		private void handle(PersonOutOfAdherenceEvent @event)
		{
			_adherencePersister.AddOut(@event.PersonId, @event.Timestamp);
		}

		private void handle(PersonInAdherenceEvent @event)
		{
			_adherencePersister.AddIn(@event.PersonId, @event.Timestamp);
		}

		private void handle(PersonNeutralAdherenceEvent @event)
		{
			_adherencePersister.AddNeutral(@event.PersonId, @event.Timestamp);
		}

		private void handle(PersonStateChangedEvent @event)
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

		private void handle(PersonRuleChangedEvent @event)
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