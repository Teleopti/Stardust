using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class RtaTracerReader : IRtaTracerReader
	{
		private readonly ICurrentAnalyticsUnitOfWork _unitOfWork;
		private readonly IJsonDeserializer _deserializer;

		public RtaTracerReader(ICurrentAnalyticsUnitOfWork unitOfWork, IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_deserializer = deserializer;
		}

		private class internalModel
		{
			public DateTime Time { get; set; }
			public string Message { get; set; }
		}

		public IEnumerable<RtaTracerLog<T>> ReadOfType<T>()
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery($@"SELECT Time, Message FROM RtaTracer WHERE MessageType = :MessageType")
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