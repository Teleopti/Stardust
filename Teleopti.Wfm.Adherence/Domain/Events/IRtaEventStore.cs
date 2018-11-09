﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public interface IRtaEventStoreUpgradeWriter
	{
		IEnumerable<UpgradeEvent> LoadForUpgrade(int fromStoreVersion, int batchSize);
		void Upgrade(UpgradeEvent @event, int toStoreVersion);
	}

	public class UpgradeEvent
	{
		public int Id;
		public IRtaStoredEvent Event;
	}

	public interface IRtaEventStore
	{
		void Add(IEvent @event, DeadLockVictim deadLockVictim, int storeVersion);

		void Add(IEnumerable<IEvent> events, DeadLockVictim deadLockVictim, int storeVersion);

		// maybe segregate this stuff
		int Remove(DateTime removeUntil, int maxEventsToRemove);
	}

	public interface IRtaEventStoreReader
	{
		IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period);
		IEnumerable<IEvent> Load(Guid personId, DateOnly date);
		[RemoveMeWithToggle(Toggles.RTA_SpeedUpHistoricalAdherence_RemoveLastBefore_78306)]
		IEvent LoadLastAdherenceEventBefore(Guid personId, DateTime timestamp, DeadLockVictim deadLockVictim);
		LoadedEvents LoadFrom(int fromEventId);
		int ReadLastId();
	}

	public class LoadedEvents
	{
		public int LastId { get; set; }
		public IEnumerable<IEvent> Events { get; set; }
	}

	public interface IRtaEventStoreTester
	{
		IEnumerable<IEvent> LoadAllForTest();
		IEnumerable<string> LoadAllEventTypeIds();
	}
}