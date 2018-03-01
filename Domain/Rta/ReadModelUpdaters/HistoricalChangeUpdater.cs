﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Rta.Events;

namespace Teleopti.Ccc.Domain.Rta.ReadModelUpdaters
{
	public class HistoricalChangeUpdater :
		IHandleEvents,
		IRunInSync
	{
		private readonly AdherenceChange.HistoricalChange _historicalChange;

		public HistoricalChangeUpdater(AdherenceChange.HistoricalChange historicalChange)
		{
			_historicalChange = historicalChange;
		}

		public void Subscribe(SubscriptionRegistrator registrator)
		{
			registrator.SubscribeTo<PersonStateChangedEvent>();
			registrator.SubscribeTo<PersonRuleChangedEvent>();
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(IEnumerable<IEvent> events) =>
			events.ForEach(e => handle((dynamic) e));

		private void handle(PersonStateChangedEvent @event)
		{
			_historicalChange.Change(new HistoricalChangeModel
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
			_historicalChange.Change(new HistoricalChangeModel
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