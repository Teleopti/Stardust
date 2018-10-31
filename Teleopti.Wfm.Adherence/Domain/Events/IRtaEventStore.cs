using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public interface IRtaEventStoreUpgradeWriter
	{
		IEnumerable<UpgradeEvent> LoadForUpgrade(int fromVersion, int batchSize);
		void Upgrade(UpgradeEvent @event, int toVersion);
	}
	
	public class UpgradeEvent
	{
		public int Id;
		public IRtaStoredEvent Event;
	}

	public interface IRtaEventStore
	{
		void Add(IEvent @event, DeadLockVictim deadLockVictim, int version);
		// maybe segregate this stuff
		int Remove(DateTime removeUntil, int maxEventsToRemove);
	}

	public interface IRtaEventStoreReader
	{
		IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period);
		IEnumerable<IEvent> Load(Guid personId, DateOnly date);
		IEvent LoadLastAdherenceEventBefore(Guid personId, DateTime timestamp, DeadLockVictim deadLockVictim);
		LoadedEvents LoadFrom(int fromEventId);
	}

	public class LoadedEvents
	{
		public int MaxId { get; set; }
		public IEnumerable<IEvent> Events { get; set; }
	}

	public interface IRtaEventStoreTester
	{
		IEnumerable<IEvent> LoadAllForTest();
		int LoadLastIdForTest();
		IEnumerable<string> LoadAllEventTypeIds();
	}
}