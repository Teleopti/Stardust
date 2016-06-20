using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

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

		public int MaxIntervalId()
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select max(interval_id) from mart.dim_interval WITH (NOLOCK)")
				.UniqueResult<short>();
		}
	}
}