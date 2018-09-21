using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public interface IRtaEventStore
	{
		void Add(IEvent @event, DeadLockVictim deadLockVictim);
		int Remove(DateTime removeUntil, int maxEventsToRemove);
	}

	public interface IRtaEventStoreReader
	{
		IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period);
		IEvent LoadLastAdherenceEventBefore(Guid personId, DateTime timestamp, DeadLockVictim deadLockVictim);
		LoadedEvents LoadFrom(int fromEventId);
	}

	public class LoadedEvents
	{
		public int MaxId { get; set; }
		public IEnumerable<IEvent> Events { get; set; }
	}

	public interface IRtaEventStoreTestReader
	{
		IEnumerable<IEvent> LoadAllForTest();
		int LoadLastIdForTest();
		IEnumerable<string>	LoadAllEventTypes();
	}
}