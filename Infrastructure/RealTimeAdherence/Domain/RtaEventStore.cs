using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain
{
	public class RtaEventStore : IRtaEventStore, IRtaEventStoreTestReader
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
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"INSERT INTO [rta].[Events] ([Event]) VALUES (:Event)")
				.SetParameter("Event", _serializer.SerializeEvent(new internalModel
				{
					Event = @event,
					Type = @event.GetType()
				}))
				.ExecuteUpdate();
		}

		public IEnumerable<IEvent> Load(Guid personId, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IEvent LoadLastBefore(Guid personId, DateTime timestamp)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IEvent> LoadAll()
		{
			return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"SELECT [Event] FROM [rta].[Events]")
					.List<string>()
					.Select(x => _deserializer.DeserializeEvent(x, typeof(internalModel)) as internalModel)
					.Select(x => x.Event)
				;
		}

		public class internalModel
		{
			public IEvent @Event;
			public Type Type;
		}
	}
}