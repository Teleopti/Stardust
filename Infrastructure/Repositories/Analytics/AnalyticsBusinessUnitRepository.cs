using System;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				select 
					[business_unit_id] {nameof(AnalyticBusinessUnit.BusinessUnitId)}
					,[business_unit_code] {nameof(AnalyticBusinessUnit.BusinessUnitCode)}
					,[business_unit_name] {nameof(AnalyticBusinessUnit.BusinessUnitName)}
					,[datasource_id] {nameof(AnalyticBusinessUnit.DatasourceId)}
					,[insert_date] {nameof(AnalyticBusinessUnit.InsertDate)}
					,[update_date] {nameof(AnalyticBusinessUnit.UpdateDate)}
					,[datasource_update_date] {nameof(AnalyticBusinessUnit.DatasourceUpdateDate)}
				from mart.dim_business_unit WITH (NOLOCK)
				where business_unit_code=:{nameof(businessUnitCode)}
				")
				.SetGuid(nameof(businessUnitCode), businessUnitCode)
				.SetResultTransformer(Transformers.AliasToBean<AnalyticBusinessUnit>())
				.UniqueResult<AnalyticBusinessUnit>();
		}

		public void AddOrUpdate(AnalyticBusinessUnit businessUnit)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				exec [mart].[etl_dim_business_unit_add_or_update]
					@business_unit_code=:{nameof(businessUnit.BusinessUnitCode)}
					,@business_unit_name=:{nameof(businessUnit.BusinessUnitName)}
					,@datasource_update_date=:{nameof(businessUnit.DatasourceUpdateDate)}
			")
			.SetParameter(nameof(businessUnit.BusinessUnitCode), businessUnit.BusinessUnitCode)
			.SetParameter(nameof(businessUnit.BusinessUnitName), businessUnit.BusinessUnitName)
			.SetParameter(nameof(businessUnit.DatasourceUpdateDate), businessUnit.DatasourceUpdateDate)
			.ExecuteUpdate();
		}
	}
}