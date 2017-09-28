using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
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

		public IEnumerable<RtaTraceLog> Read()
		{
			return _unitOfWork.Get(uow =>
				uow.Current().Session()
					.CreateSQLQuery($@"SELECT Time, Message FROM RtaTracer")
					.SetResultTransformer(Transformers.AliasToBean<RtaTraceLog>())
					.List<RtaTraceLog>()
			);
		}
	}
}