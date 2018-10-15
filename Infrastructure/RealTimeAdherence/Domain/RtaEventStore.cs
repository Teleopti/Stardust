﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain
{
	public class RtaEventStore : IRtaEventStore, IRtaEventStoreReader, IRtaEventStoreTestReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly DeadLockVictimPriority _deadLockVictimPriority;
		private readonly IJsonEventSerializer _serializer;
		private readonly IJsonEventDeserializer _deserializer;

		public RtaEventStore(
			ICurrentUnitOfWork unitOfWork,
			DeadLockVictimPriority deadLockVictimPriority,
			IJsonEventSerializer serializer,
			IJsonEventDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_deadLockVictimPriority = deadLockVictimPriority;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		public void Add(IEvent @event, DeadLockVictim deadLockVictim)
		{
			_deadLockVictimPriority.Specify(deadLockVictim);
			
			var queryData = (@event as IRtaStoredEvent).QueryData();
			var eventType = $"{@event.GetType().FullName}, {@event.GetType().Assembly.GetName().Name}";

			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
INSERT INTO [rta].[Events] (
	[Type],
	[PersonId], 
	[StartTime], 
	[EndTime],
	[Event]
) VALUES (
	:Type,
	:PersonId, 
	:StartTime, 
	:EndTime,
	:Event
)")
				.SetParameter("PersonId", queryData.PersonId)
				.SetParameter("StartTime", queryData.StartTime == DateTime.MinValue ? null : queryData.StartTime)
				.SetParameter("EndTime", queryData.EndTime == DateTime.MinValue ? null : queryData.EndTime)
				.SetParameter("Type", eventType)
				.SetParameter("Event", _serializer.SerializeEvent(@event))
				.ExecuteUpdate();
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

		public IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period) =>
			loadEvents(
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT 
	[Type],
	[Event] 
FROM 
	[rta].[Events] WITH (NOLOCK) 
WHERE 
	PersonId = :PersonId AND 
	StartTime <= :EndTime AND 
	EndTime >= :StartTime
ORDER BY [Id] ASC
")
					.SetParameter("PersonId", personId)
					.SetParameter("StartTime", period.StartDateTime)
					.SetParameter("EndTime", period.EndDateTime)
			);

		public IEvent LoadLastAdherenceEventBefore(Guid personId, DateTime timestamp, DeadLockVictim deadLockVictim)
		{
			_deadLockVictimPriority.Specify(deadLockVictim);
			
			return loadEvents(_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT TOP 1 
	[Type],
	[Event] 
FROM 
	[rta].[Events] WITH (NOLOCK)
WHERE 
	[Type] IN (:Types) AND
	PersonId = :PersonId AND 
	[EndTime] < :Timestamp
ORDER BY [Id] DESC
")
					.SetParameterList("Types", new[]
					{
						$"{typeof(PersonStateChangedEvent).FullName}, {typeof(PersonStateChangedEvent).Assembly.GetName().Name}",
						$"{typeof(PersonRuleChangedEvent).FullName}, {typeof(PersonRuleChangedEvent).Assembly.GetName().Name}"
					})
					.SetParameter("PersonId", personId)
					.SetParameter("Timestamp", timestamp))
				.SingleOrDefault();
		}

		public LoadedEvents LoadFrom(int fromEventId)
		{
			var events = load(
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT
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
				MaxId = events.IsNullOrEmpty() ? 0 : events.Last().Id,
				Events = events.Select(e => e.DeserializedEvent).ToArray()
			};
		}


		private IEnumerable<IEvent> loadEvents(IQuery query) =>
			load(query).Select(x => x.DeserializedEvent).ToArray();

		private IEnumerable<internalModel> load(IQuery query) =>
			query
				.SetResultTransformer(Transformers.AliasToBean<internalModel>())
				.List<internalModel>()
				.Select(x =>
				{
					x.DeserializedEvent = _deserializer.DeserializeEvent(x.Event, Type.GetType(x.Type)) as IEvent;
					return x;
				});


		private class internalModel
		{
#pragma warning disable 649
			public int Id;
			public string Type;
			public string Event;
			public IEvent DeserializedEvent;
#pragma warning restore 649
		}


		public IEnumerable<IEvent> LoadAllForTest() =>
			loadEvents(_unitOfWork.Current().Session().CreateSQLQuery(@"SELECT [Type], [Event] FROM [rta].[Events]"));
		
		public int LoadLastIdForTest() =>
			_unitOfWork.Current().Session().CreateSQLQuery(@"SELECT MAX([Id]) FROM [rta].[Events] WITH (NOLOCK)").UniqueResult<int>();

		public IEnumerable<string> LoadAllEventTypes() => _unitOfWork.Current().Session()
			.CreateSQLQuery(@"SELECT [Type] FROM [rta].[Events]")
			.List<string>();
	}
}