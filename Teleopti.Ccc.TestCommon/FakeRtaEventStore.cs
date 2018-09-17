using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRtaEventStore : IRtaEventStore, IRtaEventStoreReader
	{
		private readonly Lazy<IRtaEventStoreSynchronizer> _synchronizer;

		private class storedEvent
		{
			public Guid PersonId;
			public DateTimePeriod Period;
			public IEvent Event;
			public int Id;
		}

		private int _id;

		public FakeRtaEventStore(Lazy<IRtaEventStoreSynchronizer> synchronizer)
		{
			_synchronizer = synchronizer;
		}

		private readonly IList<storedEvent> _events = new List<storedEvent>();

		public void Add(IEvent @event)
		{
			var rtaStoredEvent = (@event as IRtaStoredEvent).QueryData();
			if (rtaStoredEvent == null)
				return;

			_events.Add(new storedEvent
			{
				Id = _id = _id + 1,
				PersonId = rtaStoredEvent.PersonId.Value,
				Period = new DateTimePeriod(rtaStoredEvent.StartTime.Value, rtaStoredEvent.EndTime.Value),
				Event = @event
			});

			_synchronizer.Value.Synchronize();
		}

		public int Remove(DateTime removeUntil, int maxEventsToRemove) => throw new NotImplementedException();

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

		public LoadedEvents LoadFrom(int latestSynchronizedEvent)
		{
			var events = _events
				.Where(e => e.Id > latestSynchronizedEvent)
				.Select(e => e.Event);
			
			var maxId = _events.Max(e => e.Id);
			
			return new LoadedEvents
			{
				MaxId = maxId,
				Events = events
			};
		}
	}
}