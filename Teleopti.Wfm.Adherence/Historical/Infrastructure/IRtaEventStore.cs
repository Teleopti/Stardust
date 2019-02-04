using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Historical.Infrastructure
{
	public interface IRtaEventStoreUpgradeWriter
	{
		IEnumerable<UpgradeEvent> LoadForUpgrade(int fromStoreVersion, int batchSize);
		void Upgrade(UpgradeEvent @event, int toStoreVersion);
	}

	public class UpgradeEvent
	{
		public long Id;
		public IRtaStoredEvent Event;
	}

	public interface IRtaEventStore
	{
		void Add(IEvent @event, DeadLockVictim deadLockVictim, int storeVersion);
		void Add(IEnumerable<IEvent> events, DeadLockVictim deadLockVictim, int storeVersion);
		int Remove(DateTime removeUntil, int maxEventsToRemove);
	}

	public interface IRtaEventStoreReader
	{
		IEnumerable<IEvent> Load(Guid personId, DateTime @from, DateTime to);
		IEnumerable<IEvent> Load(Guid personId, DateOnly date);
		LoadedEvents LoadForSynchronization(long fromEventId);
		long ReadLastId();
		IEnumerable<IEvent> LoadAdjustedPeriodEvents();
	}

	public class LoadedEvents
	{
		public long ToId { get; set; }
		public IEnumerable<IEvent> Events { get; set; }
	}

	public interface IRtaEventStoreTester
	{
		void AddWithoutStoreVersion(IEvent @event, DeadLockVictim deadLockVictim);
		IEnumerable<IEvent> LoadAllForTest();
		IEnumerable<string> LoadAllEventTypeIds();
	}
}