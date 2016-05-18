using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsScenarioRepository : IAnalyticsScenarioRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsScenarioRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public void AddScenario(AnalyticsScenario scenario)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_scenario_insert]
						@scenario_code=:ScenarioCode, 
						@scenario_name=:ScenarioName, 
						@default_scenario=:DefaultScenario,
						@business_unit_id=:BusinessUnitId,
						@business_unit_code=:BusinessUnitCode,
						@business_unit_name=:BusinessUnitName,
						@datasource_id=:DatasourceId,
						@datasource_update_date=:DatasourceUpdateDate,
						@is_deleted=:IsDeleted
					  ")
				.SetGuid("ScenarioCode", scenario.ScenarioCode.GetValueOrDefault())
				.SetString("ScenarioName", scenario.ScenarioName)
				.SetBoolean("DefaultScenario", scenario.DefaultScenario.GetValueOrDefault())
				.SetInt32("BusinessUnitId", scenario.BusinessUnitId)
				.SetGuid("BusinessUnitCode", scenario.BusinessUnitCode.GetValueOrDefault())
				.SetString("BusinessUnitName", scenario.BusinessUnitName)
				.SetInt32("DatasourceId", scenario.DatasourceId)
				.SetDateTime("DatasourceUpdateDate", scenario.DatasourceUpdateDate)
				.SetBoolean("IsDeleted", scenario.IsDeleted);
			query.ExecuteUpdate();
		}

		public void UpdateScenario(AnalyticsScenario scenario)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_scenario_update]
						@scenario_code=:ScenarioCode, 
						@scenario_name=:ScenarioName, 
						@default_scenario=:DefaultScenario,
						@business_unit_id=:BusinessUnitId,
						@business_unit_code=:BusinessUnitCode,
						@business_unit_name=:BusinessUnitName,
						@datasource_id=:DatasourceId,
						@datasource_update_date=:DatasourceUpdateDate,
						@is_deleted=:IsDeleted
					  ")
				.SetGuid("ScenarioCode", scenario.ScenarioCode.GetValueOrDefault())
				.SetString("ScenarioName", scenario.ScenarioName)
				.SetBoolean("DefaultScenario", scenario.DefaultScenario.GetValueOrDefault())
				.SetInt32("BusinessUnitId", scenario.BusinessUnitId)
				.SetGuid("BusinessUnitCode", scenario.BusinessUnitCode.GetValueOrDefault())
				.SetString("BusinessUnitName", scenario.BusinessUnitName)
				.SetInt32("DatasourceId", scenario.DatasourceId)
				.SetDateTime("DatasourceUpdateDate", scenario.DatasourceUpdateDate)
				.SetBoolean("IsDeleted", scenario.IsDeleted);
			query.ExecuteUpdate();
		}

		public IList<AnalyticsScenario> Scenarios()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"SELECT [scenario_id] ScenarioId
				  ,[scenario_code] ScenarioCode
				  ,[scenario_name] ScenarioName
				  ,[default_scenario] DefaultScenario
				  ,[business_unit_id] BusinessUnitId
				  ,[business_unit_code] BusinessUnitCode
				  ,[business_unit_name] BusinessUnitName
				  ,[datasource_id] DatasourceId
				  ,[insert_date] InsertDate
				  ,[update_date] UpdateDate
				  ,[datasource_update_date] DatasourceUpdateDate
				  ,[is_deleted] IsDeleted
			  FROM [mart].[dim_scenario] WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsScenario)))
				.SetReadOnly(true)
				.List<AnalyticsScenario>();
		}
	}
}