using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsTimeZoneRepository : IAnalyticsTimeZoneRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsTimeZoneRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public AnalyticsTimeZone Get(string timeZoneCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
	                time_zone_id {nameof(AnalyticsTimeZone.TimeZoneId)}
				from mart.dim_time_zone WITH (NOLOCK) where time_zone_code=:{nameof(timeZoneCode)}")
				.SetString(nameof(timeZoneCode), timeZoneCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsTimeZone)))
				.UniqueResult<AnalyticsTimeZone>();
		}
	}
}