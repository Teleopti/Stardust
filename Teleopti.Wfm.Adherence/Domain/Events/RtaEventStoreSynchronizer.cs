﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public interface IRtaEventStoreSynchronizer
	{
		void Trigger();
		void SynchronizeOnTrigger();
		void SynchronizeNow();
	}

	public class RtaEventStoreSynchronizer : IRtaEventStoreSynchronizer
	{
		private readonly IRtaEventStoreReader _events;
		private readonly IHistoricalOverviewReadModelPersister _readModels;
		private readonly IAgentAdherenceDayLoader _adherenceDayLoader;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IDistributedLockAcquirer _distributedLock;
		private readonly INow _now;

		public const string SynchronizedEventKey = "HistoricalOverviewReadModelSynchronizedEvent";

		public RtaEventStoreSynchronizer(
			IRtaEventStoreReader events,
			IHistoricalOverviewReadModelPersister readModels,
			IAgentAdherenceDayLoader adherenceDayLoader,
			IKeyValueStorePersister keyValueStore,
			IDistributedLockAcquirer distributedLock,
			INow now
		)
		{
			_events = events;
			_readModels = readModels;
			_adherenceDayLoader = adherenceDayLoader;
			_keyValueStore = keyValueStore;
			_distributedLock = distributedLock;
			_now = now;
		}

		public void Trigger() =>
			RunAt(_now.UtcDateTime());

		public void SynchronizeOnTrigger()
		{
			var runAt = RunAt();
			if (_now.UtcDateTime() < runAt) return;
			_distributedLock.TryLockForTypeOf(this, () =>
			{
				if (_now.UtcDateTime() < runAt) return;
				RunAt(_now.UtcDateTime().AddSeconds(10));
				synchronize();
			});
		}

		public void SynchronizeNow() => synchronize();

		[ReadModelUnitOfWork]
		protected virtual DateTime RunAt() =>
			new DateTime(_keyValueStore.Get("RtaEventStoreSynchronizerRunAt", 0L), DateTimeKind.Utc);

		[ReadModelUnitOfWork]
		protected virtual void RunAt(DateTime time) =>
			_keyValueStore.Update("RtaEventStoreSynchronizerRunAt", time.Ticks);

		private void synchronize()
		{
			var fromEventId = SynchronizedEventId();
			var events = LoadEvents(fromEventId);
			Synchronize(events.Events);
			UpdateSynchronizedEventId(events.LastId);
		}

		[UnitOfWork]
		protected virtual LoadedEvents LoadEvents(int fromEventId) =>
			_events.LoadFrom(fromEventId);

		[ReadModelUnitOfWork]
		protected virtual void UpdateSynchronizedEventId(int toEventId) =>
			_keyValueStore.Update(SynchronizedEventKey, toEventId.ToString());

		[ReadModelUnitOfWork]
		protected virtual int SynchronizedEventId() =>
			_keyValueStore.Get(SynchronizedEventKey, 0);

		[AllBusinessUnitsUnitOfWork]
		[FullPermissions]
		protected virtual void Synchronize(IEnumerable<IEvent> events)
		{
			var toBeSynched = events
				.Cast<IRtaStoredEvent>()
				.Select(e =>
				{
					var data = e.QueryData();
					return new
					{
						PersonId = data.PersonId.Value,
						Day = data.BelongsToDate ?? data.StartTime.Value.ToDateOnly()
					};
				})
				.Distinct()
				.ToArray();

			toBeSynched.ForEach(x => { synchronizeAdherenceDay(x.PersonId, x.Day); });
		}

		private void synchronizeAdherenceDay(Guid personId, DateOnly day)
		{
			var adherenceDay = _adherenceDayLoader.Load(personId, day);

			var lateForWork = adherenceDay.Changes().FirstOrDefault(c => c.LateForWork != null);
			var lateForWorkText = lateForWork != null ? lateForWork.LateForWork : "0";
			var minutesLateForWork = int.Parse(Regex.Replace(lateForWorkText, "[^0-9.]", ""));

			_readModels.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = day,
				WasLateForWork = lateForWork != null,
				MinutesLateForWork = minutesLateForWork,
				SecondsInAdherence = adherenceDay.SecondsInAherence(),
				SecondsOutOfAdherence = adherenceDay.SecondsOutOfAdherence(),
			});
		}
	}
}