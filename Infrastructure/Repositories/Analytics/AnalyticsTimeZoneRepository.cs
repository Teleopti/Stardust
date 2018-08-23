using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public abstract class AnalyticsTimeZoneRepositoryBase
	{
		protected readonly ICurrentAnalyticsUnitOfWork AnalyticsUnitOfWork;

		protected AnalyticsTimeZoneRepositoryBase(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			AnalyticsUnitOfWork = analyticsUnitOfWork;
		}

		public void SetUtcInUse(bool isUtcInUse)
		{
			var query = $@"SELECT CONVERT(BIT, count(1)) FROM mart.dim_time_zone WITH (NOLOCK) WHERE time_zone_code = 'UTC' AND utc_in_use =:{nameof(isUtcInUse)}";
			var dontUpdate = AnalyticsUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetBoolean(nameof(isUtcInUse), isUtcInUse)
				.UniqueResult<bool>();

			if (dontUpdate)
				return;

			query = $@"UPDATE mart.dim_time_zone SET utc_in_use=:{nameof(isUtcInUse)} WHERE time_zone_code = 'UTC'";
			AnalyticsUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetBoolean(nameof(isUtcInUse), isUtcInUse)
				.ExecuteUpdate();
		}

		public void SetToBeDeleted(string timeZoneCode, bool tobeDeleted)
		{
			var query = $@"SELECT CONVERT(BIT, count(1)) FROM mart.dim_time_zone WITH (NOLOCK) WHERE time_zone_code=:{nameof(timeZoneCode)} AND to_be_deleted=:{nameof(tobeDeleted)}";
			var dontUpdate = AnalyticsUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetString(nameof(timeZoneCode), timeZoneCode)
				.SetBoolean(nameof(tobeDeleted), tobeDeleted)
				.UniqueResult<bool>();

			if (dontUpdate)
				return;

			query = $@"UPDATE mart.dim_time_zone SET to_be_deleted=:{nameof(tobeDeleted)} WHERE time_zone_code=:{nameof(timeZoneCode)}";
			AnalyticsUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetString(nameof(timeZoneCode), timeZoneCode)
				.SetBoolean(nameof(tobeDeleted), tobeDeleted)
				.ExecuteUpdate();
		}

		public AnalyticsTimeZone Get(string timeZoneCode)
		{
			return AnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
	                time_zone_id {nameof(AnalyticsTimeZone.TimeZoneId)},
					time_zone_code {nameof(AnalyticsTimeZone.TimeZoneCode)},
					utc_in_use {nameof(AnalyticsTimeZone.IsUtcInUse)},
					to_be_deleted {nameof(AnalyticsTimeZone.ToBeDeleted)}
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
					time_zone_code {nameof(AnalyticsTimeZone.TimeZoneCode)},
					utc_in_use {nameof(AnalyticsTimeZone.IsUtcInUse)},
					to_be_deleted {nameof(AnalyticsTimeZone.ToBeDeleted)}
				from mart.dim_time_zone WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsTimeZone)))
				.List<AnalyticsTimeZone>();
		}

		public IList<AnalyticsTimeZone> GetAllUsedByLogDataSourcesAndBaseConfig()
		{
			return AnalyticsUnitOfWork.Current()
				.Session()
				.CreateSQLQuery(@"exec mart.sys_get_timezones_used_by_datasource_and_base_config")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsTimeZone)))
				.List<AnalyticsTimeZone>();
		}
	}
}