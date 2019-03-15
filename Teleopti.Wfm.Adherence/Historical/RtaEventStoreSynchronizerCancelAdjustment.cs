using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;

namespace Teleopti.Wfm.Adherence.Historical
{
	public class RtaEventStoreSynchronizerCancelAdjustment : IRtaEventStoreSynchronizer
	{
		private readonly IRtaEventStoreReader _events;
		private readonly IHistoricalOverviewReadModelPersister _readModels;
		private readonly IAgentAdherenceDayLoader _adherenceDayLoader;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IDistributedLockAcquirer _distributedLock;

		public const string SynchronizedEventKey = "HistoricalOverviewReadModelSynchronizedEvent";

		public RtaEventStoreSynchronizerCancelAdjustment(
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
				if (ShouldReSync())
					UpdateSynchronizedEventId(0);
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
		protected virtual bool ShouldReSync() =>
			_events.AnyEventsOfType<PeriodAdjustedToNeutralEvent>(FromEventId()) ||
			_events.AnyEventsOfType<PeriodAdjustmentToNeutralCanceledEvent>(FromEventId());

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
			synchronizationKeysForEvents(events)
				.ForEach(synchronizeHistoricalOverviewReadModel);
		}

		private static IEnumerable<synchronizationKey> synchronizationKeysForEvents(IEnumerable<IEvent> events) =>
			events
				.Where(x => x is ISynchronizationInfo)
				.Cast<ISynchronizationInfo>()
				.Select(e =>
				{
					var synchronizationInfo = e.SynchronizationInfo();
					return new synchronizationKey
					{
						PersonId = synchronizationInfo.PersonId,
						Day = synchronizationInfo.BelongsToDate ?? new DateOnly(synchronizationInfo.StartTime)
					};
				})
				.Distinct();

		private class synchronizationKey
		{
			public Guid PersonId;
			public DateOnly Day;

			protected bool Equals(synchronizationKey other)
			{
				return PersonId.Equals(other.PersonId) && Day.Equals(other.Day);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((synchronizationKey) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (PersonId.GetHashCode() * 397) ^ Day.GetHashCode();
				}
			}
		}

		private void synchronizeHistoricalOverviewReadModel(synchronizationKey key)
		{
			var adherenceDay = _adherenceDayLoader.Load(key.PersonId, key.Day);

			var lateForWork = adherenceDay.Changes().FirstOrDefault(c => c.LateForWorkMinutes.HasValue);

			_readModels.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = key.PersonId,
				Date = key.Day,
				WasLateForWork = lateForWork != null,
				MinutesLateForWork = lateForWork?.LateForWorkMinutes ?? 0,
				SecondsInAdherence = adherenceDay.SecondsInAdherence(),
				SecondsOutOfAdherence = adherenceDay.SecondsOutOfAdherence(),
			});
		}
	}
}