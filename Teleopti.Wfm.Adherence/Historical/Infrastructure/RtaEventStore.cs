using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Infrastructure;

namespace Teleopti.Wfm.Adherence.Historical.Infrastructure
{
	public class RtaEventStore : IRtaEventStore, IRtaEventStoreReader, IRtaEventStoreTester, IRtaEventStoreUpgradeWriter
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly DeadLockVictimPriority _deadLockVictimPriority;
		private readonly IJsonEventSerializer _serializer;
		private readonly IJsonEventDeserializer _deserializer;
		private readonly PersistedTypeMapper _typeMapper;
		private readonly int _addBatchSize;
		private readonly int _loadForSynchronizationSize;

		public RtaEventStore(
			ICurrentUnitOfWork unitOfWork,
			DeadLockVictimPriority deadLockVictimPriority,
			IJsonEventSerializer serializer,
			IJsonEventDeserializer deserializer,
			IConfigReader config,
			PersistedTypeMapper typeMapper)
		{
			_unitOfWork = unitOfWork;
			_deadLockVictimPriority = deadLockVictimPriority;
			_serializer = serializer;
			_deserializer = deserializer;
			_typeMapper = typeMapper;
			_addBatchSize = config.ReadValue("RtaEventStoreBatchSize", 10);
			_loadForSynchronizationSize = config.ReadValue("RtaEventStoreLoadForSynchronizationSize", 50000);
		}

		public void Add(IEvent @event, DeadLockVictim deadLockVictim, int storeVersion) =>
			add(new[] {@event}, deadLockVictim, storeVersion);

		public void Add(IEnumerable<IEvent> events, DeadLockVictim deadLockVictim, int storeVersion) => 
			add(events, deadLockVictim, storeVersion);
		
		private void add(IEnumerable<IEvent> events, DeadLockVictim deadLockVictim, int? storeVersion)
		{
			_deadLockVictimPriority.Specify(deadLockVictim);

			events.Batch(_addBatchSize).ForEach(batch =>
			{
				var sqlValues = batch.Select((m, i) =>
					$@"
(
:PersonId{i},
:BelongsToDate{i},
:StartTime{i},
:EndTime{i},
:StoreVersion,
:Type{i},
:Event{i}
)").Aggregate((current, next) => current + ", " + next);

				var query = _unitOfWork.Current().Session()
					.CreateSQLQuery($@"
INSERT INTO [rta].[Events] (
	[PersonId], 
	[BelongsToDate], 
	[StartTime], 
	[EndTime],
	[StoreVersion],
	[Type],
	[Event]
) VALUES {sqlValues}");

				batch.Select((e, i) => new {e, i})
					.ForEach(x =>
					{
						var queryData = (x.e as IRtaStoredEvent).QueryData();
						var i = x.i;
						var @event = x.e;
						query
							.SetParameter("PersonId" + i, queryData.PersonId)
							.SetParameter("BelongsToDate" + i, queryData.BelongsToDate?.Date)
							.SetParameter("StartTime" + i, queryData.StartTime == DateTime.MinValue ? null : queryData.StartTime)
							.SetParameter("EndTime" + i, queryData.EndTime == DateTime.MinValue ? null : queryData.EndTime)
							.SetParameter("Type" + i, _typeMapper.NameForPersistence(@event.GetType()))
							.SetParameter("Event" + i, _serializer.SerializeEvent(@event));
					});

				query
					.SetParameter("StoreVersion", storeVersion)
					.ExecuteUpdate();
			});
		}

		public int Remove(DateTime until, int maxEventsToRemove) =>
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
DELETE 
	TOP (:nRows) 
FROM 
	[rta].[Events] 
WHERE 
	EndTime < :ts
")
				.SetParameter("ts", until)
				.SetParameter("nRows", maxEventsToRemove)
				.ExecuteUpdate();

		public IEnumerable<UpgradeEvent> LoadForUpgrade(int fromStoreVersion, int batchSize)
		{
			return load(
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT
	[Id],
	[Type],
	[Event] 
FROM 
	[rta].[Events]
WHERE
	ISNULL(StoreVersion, :firstVersion) = :fromVersion
")
					.SetParameter("fromVersion", fromStoreVersion)
					.SetParameter("firstVersion", RtaEventStoreVersion.WithoutBelongsToDate)
					.SetMaxResults(batchSize)
			).Select(e => new UpgradeEvent
			{
				Id = e.Id,
				Event = e.DeserializedEvent as IRtaStoredEvent
			}).ToArray();
		}

		public void Upgrade(UpgradeEvent @event, int toStoreVersion)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
UPDATE [rta].[Events] 
SET 
	[Event] = :Event, 
	StoreVersion = :toStoreVersion,
	BelongsToDate = :BelongsToDate 
WHERE Id = :Id
")
				.SetParameter("toStoreVersion", toStoreVersion)
				.SetParameter("Id", @event.Id)
				.SetParameter("BelongsToDate", @event.Event.QueryData().BelongsToDate?.Date)
				.SetParameter("Event", _serializer.SerializeEvent(@event.Event))
				.ExecuteUpdate();
		}

		public IEnumerable<IEvent> Load(Guid personId, DateOnly date) =>
			loadEvents(
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT 
	[Type],
	[Event] 
FROM 
	[rta].[Events] WITH (NOLOCK)
WHERE
	PersonId = :personId AND
	BelongsToDate = :date
ORDER BY [Id] ASC
")
					.SetParameter("personId", personId)
					.SetParameter("date", date.Date)
			);

		public IEnumerable<IEvent> LoadAllOfType<T>() =>
			loadEvents(
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT 
	[Type],
	[Event] 
FROM 
	[rta].[Events] WITH (NOLOCK)
WHERE
	Type = :eventType
ORDER BY [Id] ASC
")
					.SetParameter("eventType", _typeMapper.NameForPersistence(typeof(T)))
			);

		public LoadedEvents LoadForSynchronization(long fromEventId)
		{
			var events = load(
				_unitOfWork.Current().Session()
					.CreateSQLQuery($@"
SELECT TOP {_loadForSynchronizationSize}
	[Id], 
	[Type],
	[Event] 
FROM 
	[rta].[Events]
WHERE
	[Id] > :FromEventId
ORDER BY [Id]
")
					.SetParameter("FromEventId", fromEventId)
			);
			return new LoadedEvents
			{
				ToId = events.IsNullOrEmpty() ? fromEventId : events.Last().Id,
				Events = events.Select(e => e.DeserializedEvent).ToArray()
			};
		}

		public long ReadLastId() =>
			_unitOfWork.Current().Session().CreateSQLQuery(@"SELECT MAX([Id]) FROM [rta].[Events] WITH (NOLOCK)").UniqueResult<int>();

		public bool EventsOfTypeWereStoredFromId<T>(long fromEventId)
		{
			var count = _unitOfWork.Current().Session().CreateSQLQuery(@"SELECT COUNT (*) 
FROM 
	[rta].[Events] WITH (NOLOCK)
WHERE
	[Type] = :eventType AND 
	[Id] > :fromEventId
")
				.SetParameter("eventType", _typeMapper.NameForPersistence(typeof(T)))
				.SetParameter("fromEventId", fromEventId)
				.UniqueResult<int>();
			return count != 0;
		}

		private IEnumerable<IEvent> loadEvents(IQuery query) =>
			load(query).Select(x => x.DeserializedEvent).ToArray();

		private IEnumerable<internalModel> load(IQuery query) =>
			query
				.SetResultTransformer(Transformers.AliasToBean<internalModel>())
				.List<internalModel>()
				.Select(x =>
				{
					x.DeserializedEvent = _deserializer.DeserializeEvent(x.Event, _typeMapper.TypeForPersistedName(x.Type)) as IEvent;
					return x;
				});


		private class internalModel
		{
#pragma warning disable 649
			public long Id;
			public string Type;
			public string Event;
#pragma warning restore 649
			public IEvent DeserializedEvent;
		}

		public void AddWithoutStoreVersion(IEvent @event, DeadLockVictim deadLockVictim) =>
			add(new[] {@event}, deadLockVictim, null);
		
		public IEnumerable<IEvent> LoadAllForTest() =>
			loadEvents(_unitOfWork.Current().Session().CreateSQLQuery(@"SELECT [Type], [Event] FROM [rta].[Events]"));

		public IEnumerable<string> LoadAllEventTypes() =>
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"SELECT [Type] FROM [rta].[Events]")
				.List<string>();
	}
}