using System;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsBusinessUnitRepository : IAnalyticsBusinessUnitRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsBusinessUnitRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}
		public AnalyticBusinessUnit Get(Guid businessUnitCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(@"
				select 
					business_unit_id BusinessUnitId
					,datasource_id DatasourceId
				from mart.dim_business_unit WITH (NOLOCK)
				where business_unit_code=:BusinessUnitCode
				")
				.SetGuid("BusinessUnitCode", businessUnitCode)
				.SetResultTransformer(Transformers.AliasToBean<AnalyticBusinessUnit>())
				.UniqueResult<AnalyticBusinessUnit>();
		}
	}
}