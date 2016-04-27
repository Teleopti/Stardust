using System;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsBusinessUnitRepository : IAnalyticsBusinessUnitRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public AnalyticsBusinessUnitRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public AnalyticBusinessUnit Get(Guid businessUnitCode)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(@"
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
}