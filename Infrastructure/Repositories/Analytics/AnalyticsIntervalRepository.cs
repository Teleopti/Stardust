using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsIntervalRepository : IAnalyticsIntervalRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsIntervalRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public int IntervalsPerDay()
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select count(1) from mart.dim_interval WITH (NOLOCK)")
				.UniqueResult<int>();
		}

		public AnalyticsInterval MaxInterval()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select top 1
						interval_id  {nameof(AnalyticsInterval.IntervalId)},
						interval_start {nameof(AnalyticsInterval.IntervalStart)}
					from mart.dim_interval WITH (NOLOCK)
					ORDER BY interval_id desc")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsInterval)))
				.SetReadOnly(true)
				.UniqueResult<AnalyticsInterval>();
		}

		public IList<AnalyticsInterval> GetAll()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
						interval_id {nameof(AnalyticsInterval.IntervalId)},
						interval_start {nameof(AnalyticsInterval.IntervalStart)}
					from mart.dim_interval WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsInterval)))
				.SetReadOnly(true)
				.List<AnalyticsInterval>();
		}
	}
}