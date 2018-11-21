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
		void Synchronize();
	}

	public class DontSynchronize : IRtaEventStoreSynchronizer, IRtaEventStoreAsyncSynchronizer
	{
		public void Synchronize()
		{
		}

		public void SynchronizeAsync()
		{
		}
	}

	public class RtaEventStoreSynchronizer : IRtaEventStoreSynchronizer
	{
		private readonly IRtaEventStoreReader _events;
		private readonly IHistoricalOverviewReadModelPersister _readModels;
		private readonly IAgentAdherenceDayLoader _adherenceDayLoader;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IDistributedLockAcquirer _distributedLock;

		public const string SynchronizedEventKey = "HistoricalOverviewReadModelSynchronizedEvent";

		public RtaEventStoreSynchronizer(
			IRtaEventStoreReader events,
			IHistoricalOverviewReadModelPersister readModels,
			IAgentAdherenceDayLoader adherenceDayLoader,
			IKeyValueStorePersister keyValueStore,
			IDistributedLockAcquirer distributedLock
		)
		{
			_events = events;
			_readModels = readModels;
			_adherenceDayLoader = adherenceDayLoader;
			_keyValueStore = keyValueStore;
			_distributedLock = distributedLock;
		}

		[TestLog]
		public virtual void Synchronize()
		{
			_distributedLock.TryLockForTypeOf(this, () =>
			{
				var toEventId = ToEventId();
				while (true)
				{
					var fromEventId = FromEventId();
					var events = LoadEvents(fromEventId);
					Synchronize(events.Events);
					UpdateSynchronizedEventId(events.ToId);
					if (events.ToId >= toEventId)
						break;
				}
			});
		}

		[UnitOfWork]
		protected virtual LoadedEvents LoadEvents(long fromEventId) =>
			_events.LoadForSynchronization(fromEventId);

		[TestLog]
		[ReadModelUnitOfWork]
		protected virtual long FromEventId() =>
			_keyValueStore.Get(SynchronizedEventKey, 0);

		[TestLog]
		[UnitOfWork]
		protected virtual long ToEventId() =>
			_events.ReadLastId();

		[TestLog]
		[ReadModelUnitOfWork]
		protected virtual void UpdateSynchronizedEventId(long toEventId) =>
			_keyValueStore.Update(SynchronizedEventKey, toEventId.ToString());

		[TestLog]
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

			Extensions.ForEach(toBeSynched, x => { synchronizeAdherenceDay(x.PersonId, x.Day); });
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