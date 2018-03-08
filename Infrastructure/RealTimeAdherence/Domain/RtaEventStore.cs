using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain
{
	public class RtaEventStore : IRtaEventStore, IRtaEventStoreTestReader, IRtaEventStoreReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IJsonEventSerializer _serializer;
		private readonly IJsonEventDeserializer _deserializer;

		public RtaEventStore(
			ICurrentUnitOfWork unitOfWork,
			IJsonEventSerializer serializer,
			IJsonEventDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		public void Add(IEvent @event)
		{
			var queryData = (@event as IRtaStoredEvent).QueryData();

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
				.SetParameter("Type", @event.GetType().AssemblyQualifiedName)
				.SetParameter("Event", _serializer.SerializeEvent(@event))
				.ExecuteUpdate();
		}

		public IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period) =>
			load(
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT 
	[Type],
	[Event] 
FROM 
	[rta].[Events] 
WHERE 
	PersonId = :PersonId AND 
	StartTime <= :EndTime AND 
	EndTime >= :StartTime
")
					.SetParameter("PersonId", personId)
					.SetParameter("StartTime", period.StartDateTime)
					.SetParameter("EndTime", period.EndDateTime)
			);

		public IEvent LoadLastAdherenceEventBefore(Guid personId, DateTime timestamp)
		{
			return load(_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT TOP 1 
	[Type],
	[Event] 
FROM 
	[rta].[Events] 
WHERE 
	[Type] IN (:Types) AND
	PersonId = :PersonId AND 
	[EndTime] < :Timestamp
	ORDER BY [StartTime] DESC
")
					.SetParameterList("Types", new[]
					{
						typeof(PersonStateChangedEvent).AssemblyQualifiedName,
						typeof(PersonRuleChangedEvent).AssemblyQualifiedName
					})
					.SetParameter("PersonId", personId)
					.SetParameter("Timestamp", timestamp))
				.SingleOrDefault();
		}

		public IEnumerable<IEvent> LoadAll() =>
			load(
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT 
	[Type],
	[Event] 
FROM 
	[rta].[Events] 
")
			);

		private IEnumerable<IEvent> load(IQuery query) =>
			query
				.SetResultTransformer(Transformers.AliasToBean<internalModel>())
				.List<internalModel>()
				.Select(x => _deserializer.DeserializeEvent(x.Event, Type.GetType(x.Type)))
				.Cast<IEvent>()
				.ToArray();

		private class internalModel
		{
#pragma warning disable 649
			public string Type;
			public string Event;
#pragma warning restore 649
		}
	}
}