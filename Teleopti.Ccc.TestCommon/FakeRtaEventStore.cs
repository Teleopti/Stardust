﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRtaEventStore : IRtaEventStore, IRtaEventStoreReader, IRtaEventStoreUpgradeWriter
	{
		private readonly Lazy<IRtaEventStoreSynchronizer> _synchronizer;

		public class StoredEvent
		{
			public int Id;
			public int StoreVersion;
			public Guid PersonId;
			public DateOnly? BelongsToDate;
			public DateTime StartTime;
			public DateTime EndTime;
			public IEvent Event;
		}

		private int _id;

		public FakeRtaEventStore(Lazy<IRtaEventStoreSynchronizer> synchronizer)
		{
			_synchronizer = synchronizer;
		}

		public IEnumerable<StoredEvent> Data = Enumerable.Empty<StoredEvent>();

		public void Add(IEvent @event, DeadLockVictim deadLockVictim, int storeVersion) => 
			Add(new[] {@event}, deadLockVictim, storeVersion);
		
		public void Add(IEnumerable<IEvent> events, DeadLockVictim deadLockVictim, int storeVersion)
		{
			events.ForEach(@event =>
				{
					var queryData = (@event as IRtaStoredEvent).QueryData();
					if (queryData == null)
						return;

					Data = Data.Append(new StoredEvent
					{
						Id = _id = _id + 1,
						StoreVersion = storeVersion,
						PersonId = queryData.PersonId.Value,
						BelongsToDate = queryData.BelongsToDate,
						StartTime = queryData.StartTime.Value,
						EndTime = queryData.EndTime.Value,
						Event = @event
					}).ToArray();
				}
			);
			
			_synchronizer.Value.Synchronize();
		}

		public int Remove(DateTime removeUntil, int maxEventsToRemove) => throw new NotImplementedException();

		public IEnumerable<UpgradeEvent> LoadForUpgrade(int fromStoreVersion, int batchSize)
		{
			return Data
				.Where(x => x.StoreVersion == fromStoreVersion)
				.Select(e => new UpgradeEvent
				{
					Id = e.Id,
					Event = e.Event.CopyBySerialization(e.Event.GetType()) as IRtaStoredEvent
				}).ToArray();
		}

		public void Upgrade(UpgradeEvent @event, int toStoreVersion)
		{
			var toUpdate = Data.Single(e => e.Id == @event.Id);
			toUpdate.Event = @event.Event as IEvent;
			toUpdate.StoreVersion = toStoreVersion;
		}

		public IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period)
		{
			return Data
				.Where(x => x.PersonId == personId &&
							x.StartTime <= period.EndDateTime &&
							x.EndTime >= period.StartDateTime)
				.Select(e => e.Event)
				.ToArray();
		}

		public IEnumerable<IEvent> Load(Guid personId, DateOnly date)
		{
			return Data
				.Where(x => x.PersonId == personId &&
							x.BelongsToDate == date)
				.Select(e => e.Event)
				.ToArray();
		}

		public IEvent LoadLastAdherenceEventBefore(Guid personId, DateTime timestamp, DeadLockVictim deadLockVictim)
		{
			return Data
				.Where(e => e.PersonId == personId &&
							e.EndTime < timestamp)
				.OrderBy(x => x.EndTime)
				.LastOrDefault()
				?.Event;
		}

		public LoadedEvents LoadFrom(int latestSynchronizedEvent)
		{
			var events = Data
				.Where(e => e.Id > latestSynchronizedEvent)
				.Select(e => e.Event.CopyBySerialization(e.Event.GetType()))
				.Cast<IEvent>()
				.ToArray();

			var maxId = Data.Max(e => e.Id);

			return new LoadedEvents
			{
				LastId = maxId,
				Events = events
			};
		}

		public int ReadLastId()
		{
			throw new NotImplementedException();
		}
	}
}