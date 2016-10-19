using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsTimeZoneRepository : AnalyticsTimeZoneRepositoryBase, IAnalyticsTimeZoneRepository
	{
		public AnalyticsTimeZoneRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork) : base(analyticsUnitOfWork)
		{
		}
	}

	public abstract class AnalyticsTimeZoneRepositoryBase
	{
		protected readonly ICurrentAnalyticsUnitOfWork AnalyticsUnitOfWork;

		protected AnalyticsTimeZoneRepositoryBase(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			AnalyticsUnitOfWork = analyticsUnitOfWork;
		}

		public AnalyticsTimeZone Get(string timeZoneCode)
		{
			return AnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
	                time_zone_id {nameof(AnalyticsTimeZone.TimeZoneId)},
					time_zone_code {nameof(AnalyticsTimeZone.TimeZoneCode)}
				from mart.dim_time_zone WITH (NOLOCK) where time_zone_code=:{nameof(timeZoneCode)}")
				.SetString(nameof(timeZoneCode), timeZoneCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsTimeZone)))
				.UniqueResult<AnalyticsTimeZone>();
		}

		public IList<AnalyticsTimeZone> GetAll()
		{
			return AnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
	                time_zone_id {nameof(AnalyticsTimeZone.TimeZoneId)},
					time_zone_code {nameof(AnalyticsTimeZone.TimeZoneCode)}
				from mart.dim_time_zone WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsTimeZone)))
				.List<AnalyticsTimeZone>();
		}
	}
}