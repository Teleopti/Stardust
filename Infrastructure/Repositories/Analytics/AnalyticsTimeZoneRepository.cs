using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsTimeZoneRepository : IAnalyticsTimeZoneRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public AnalyticsTimeZoneRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public AnalyticsTimeZone Get(string id)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					@"select 
	                    time_zone_id TimeZoneId
					from mart.dim_time_zone WITH (NOLOCK) where time_zone_code=:TimeZoneCode")
					.SetString("TimeZoneCode", id)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsTimeZone)))
					.UniqueResult<AnalyticsTimeZone>();
			}
		}
	}
}