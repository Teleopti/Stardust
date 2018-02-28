using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
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
	}

	public class HistoricalAdherenceUpdater :
		IHandleEvents,
		IRunInSync
	{
		private readonly AdherenceChange.AdherenceChange _adherenceChange;

		public HistoricalAdherenceUpdater(AdherenceChange.AdherenceChange adherenceChange)
		{
			_adherenceChange = adherenceChange;
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
		public virtual void Handle(IEnumerable<IEvent> events) =>
			events.ForEach(e => handle((dynamic) e));

		private void handle(PersonOutOfAdherenceEvent @event) =>
			_adherenceChange.Out(@event.PersonId, @event.Timestamp);

		private void handle(PersonInAdherenceEvent @event) =>
			_adherenceChange.In(@event.PersonId, @event.Timestamp);

		private void handle(PersonNeutralAdherenceEvent @event) =>
			_adherenceChange.Neutral(@event.PersonId, @event.Timestamp);

		private void handle(PersonStateChangedEvent @event)
		{
			_adherenceChange.Change(new HistoricalChange
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
			_adherenceChange.Change(new HistoricalChange
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

		private static HistoricalChangeAdherence? adherenceFor(EventAdherence? eventAdherence)
		{
			if (!eventAdherence.HasValue)
				return null;

			switch (eventAdherence.Value)
			{
				case EventAdherence.In:
					return HistoricalChangeAdherence.In;
				case EventAdherence.Out:
					return HistoricalChangeAdherence.Out;
				case EventAdherence.Neutral:
					return HistoricalChangeAdherence.Neutral;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}