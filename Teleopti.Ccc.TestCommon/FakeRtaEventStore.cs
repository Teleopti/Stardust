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
			var rtaStoredEvent = (@event as IRtaStoredEvent).QueryData();
			if (rtaStoredEvent == null)
				return;
			
			_events.Add(new storedEvent
			{
				PersonId = rtaStoredEvent.PersonId.Value,
				Period = new DateTimePeriod(rtaStoredEvent.StartTime.Value, rtaStoredEvent.EndTime.Value),
				Event = @event
			});
		}

		public void Remove(DateTime removeUntil)
		{
		}

		public IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period)
		{
			return _events
				.Where(x => x.PersonId == personId &&
							x.Period.StartDateTime <= period.EndDateTime &&
							x.Period.EndDateTime >= period.StartDateTime)
				.Select(e => e.Event);
		}

		public IEvent LoadLastAdherenceEventBefore(Guid personId, DateTime timestamp)
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