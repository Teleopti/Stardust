using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class RtaTracerReader : IRtaTracerReader
	{
		private readonly WithAnalyticsUnitOfWork _unitOfWork;

		public RtaTracerReader(WithAnalyticsUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		private class internalModel
		{
			public DateTime Time { get; set; }
			public string Message { get; set; }
		}
		
		public IEnumerable<RtaTracerLog<T>> ReadOfType<T>()
		{
			return _unitOfWork.Get(uow =>
				uow.Current().Session()
					.CreateSQLQuery($@"SELECT Time, Message FROM RtaTracer")
					.SetResultTransformer(Transformers.AliasToBean<internalModel>())
					.List<internalModel>()
			).Select(x => new RtaTracerLog<T>()
			{
				Time = x.Time,
				Message = x.Message
			}).ToArray();
		}
	}
}