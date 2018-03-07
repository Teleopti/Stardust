using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRtaEventStore : IRtaEventStore, IRtaEventStoreReader
	{
		private class storedEvent
		{
			public Guid PersonId;
			public DateTimePeriod Period;
			public IEvent Event;
		}

		private readonly IList<storedEvent> _events = new List<storedEvent>();

		public void Add(IEvent @event)
		{
			if (@event is PersonStateChangedEvent e)
				_events.Add(new storedEvent
				{
					PersonId = e.PersonId,
					Period = new DateTimePeriod(e.Timestamp, e.Timestamp),
					Event = e
				});

			if (@event is PersonRuleChangedEvent ev)
				_events.Add(new storedEvent
				{
					PersonId = ev.PersonId,
					Period = new DateTimePeriod(ev.Timestamp, ev.Timestamp),
					Event = ev
				});

			if (@event is PeriodApprovedAsInAdherenceEvent eve)
				_events.Add(new storedEvent
				{
					PersonId = eve.PersonId,
					Period = new DateTimePeriod(eve.StartTime, eve.EndTime),
					Event = eve
				});
		}

		public IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period)
		{
			return _events
				.Where(x => x.PersonId == personId &&
							x.Period.StartDateTime <= period.EndDateTime &&
							x.Period.EndDateTime >= period.StartDateTime)
				.Select(e => e.Event);
		}

		public IEvent LoadLastBefore(Guid personId, DateTime timestamp)
		{
			return _events
				.Where(e => e.PersonId == personId &&
							e.Period.EndDateTime < timestamp)
				.OrderBy(x => x.Period.EndDateTime)
				.LastOrDefault()
				?.Event;
		}
	}
}