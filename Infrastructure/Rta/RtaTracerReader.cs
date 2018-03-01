using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.RealTimeAdherence.Tracer;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class RtaTracerReader : IRtaTracerReader
	{
		private readonly IJsonDeserializer _deserializer;
		private readonly ICurrentDataSource _dataSource;
		private readonly RtaTracerSessionFactory _sessionFactory;

		public RtaTracerReader(IJsonDeserializer deserializer, ICurrentDataSource dataSource, RtaTracerSessionFactory sessonFactory)
		{
			_deserializer = deserializer;
			_dataSource = dataSource;
			_sessionFactory = sessonFactory;
		}

		private class internalModel
		{
			public DateTime Time { get; set; }
			public string Message { get; set; }
		}

		public IEnumerable<RtaTracerLog<T>> ReadOfType<T>()
		{
			using (var session = _sessionFactory.OpenSession())
				return session
					.CreateSQLQuery($@"SELECT Time, Message FROM RtaTracer.Logs WHERE MessageType = :MessageType AND Tenant = :Tenant")
					.SetParameter("Tenant", _dataSource.CurrentName())
					.SetParameter("MessageType", typeof(T).Name)
					.SetResultTransformer(Transformers.AliasToBean<internalModel>())
					.List<internalModel>()
					.Select(x =>
					{
						var model = _deserializer.DeserializeObject<RtaTracerLog<T>>(x.Message);
						model.Time = x.Time;
						return model;
					})
					.ToArray();
		}
		
	}
}