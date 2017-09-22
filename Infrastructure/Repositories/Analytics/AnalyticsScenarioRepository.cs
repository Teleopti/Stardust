using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
				$@"exec mart.[etl_dim_scenario_insert]
						@scenario_code=:{nameof(scenario.ScenarioCode)} 
						,@scenario_name=:{nameof(scenario.ScenarioName)} 
						,@default_scenario=:{nameof(scenario.DefaultScenario)} 
						,@business_unit_id=:{nameof(scenario.BusinessUnitId)} 
						,@business_unit_code=:{nameof(scenario.BusinessUnitCode)} 
						,@business_unit_name=:{nameof(scenario.BusinessUnitName)} 
						,@datasource_id=:{nameof(scenario.DatasourceId)} 
						,@datasource_update_date=:{nameof(scenario.DatasourceUpdateDate)} 
						,@is_deleted=:{nameof(scenario.IsDeleted)} 
					  ")
				.SetParameter(nameof(scenario.ScenarioCode), scenario.ScenarioCode.GetValueOrDefault())
				.SetParameter(nameof(scenario.ScenarioName), scenario.ScenarioName)
				.SetParameter(nameof(scenario.DefaultScenario), scenario.DefaultScenario.GetValueOrDefault())
				.SetParameter(nameof(scenario.BusinessUnitId), scenario.BusinessUnitId)
				.SetParameter(nameof(scenario.BusinessUnitCode), scenario.BusinessUnitCode.GetValueOrDefault())
				.SetParameter(nameof(scenario.BusinessUnitName), scenario.BusinessUnitName)
				.SetParameter(nameof(scenario.DatasourceId), scenario.DatasourceId)
				.SetParameter(nameof(scenario.DatasourceUpdateDate), scenario.DatasourceUpdateDate)
				.SetParameter(nameof(scenario.IsDeleted), scenario.IsDeleted);
			query.ExecuteUpdate();
		}

		public void UpdateScenario(AnalyticsScenario scenario)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_scenario_update]
						@scenario_code=:{nameof(scenario.ScenarioCode)} 
						,@scenario_name=:{nameof(scenario.ScenarioName)} 
						,@default_scenario=:{nameof(scenario.DefaultScenario)}
						,@business_unit_id=:{nameof(scenario.BusinessUnitId)} 
						,@business_unit_code=:{nameof(scenario.BusinessUnitCode)} 
						,@business_unit_name=:{nameof(scenario.BusinessUnitName)} 
						,@datasource_id=:{nameof(scenario.DatasourceId)} 
						,@datasource_update_date=:{nameof(scenario.DatasourceUpdateDate)} 
						,@is_deleted=:{nameof(scenario.IsDeleted)} 
					  ")
				.SetParameter(nameof(scenario.ScenarioCode), scenario.ScenarioCode.GetValueOrDefault())
				.SetParameter(nameof(scenario.ScenarioName), scenario.ScenarioName)
				.SetParameter(nameof(scenario.DefaultScenario), scenario.DefaultScenario.GetValueOrDefault())
				.SetParameter(nameof(scenario.BusinessUnitId), scenario.BusinessUnitId)
				.SetParameter(nameof(scenario.BusinessUnitCode), scenario.BusinessUnitCode.GetValueOrDefault())
				.SetParameter(nameof(scenario.BusinessUnitName), scenario.BusinessUnitName)
				.SetParameter(nameof(scenario.DatasourceId), scenario.DatasourceId)
				.SetParameter(nameof(scenario.DatasourceUpdateDate), scenario.DatasourceUpdateDate)
				.SetParameter(nameof(scenario.IsDeleted), scenario.IsDeleted);
			query.ExecuteUpdate();
		}

		public IList<AnalyticsScenario> Scenarios()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT 
						[scenario_id] {nameof(AnalyticsScenario.ScenarioId)}
						,[scenario_code] {nameof(AnalyticsScenario.ScenarioCode)}
						,[scenario_name] {nameof(AnalyticsScenario.ScenarioName)}
						,[default_scenario] {nameof(AnalyticsScenario.DefaultScenario)}
						,[business_unit_id] {nameof(AnalyticsScenario.BusinessUnitId)}
						,[business_unit_code] {nameof(AnalyticsScenario.BusinessUnitCode)}
						,[business_unit_name] {nameof(AnalyticsScenario.BusinessUnitName)}
						,[datasource_id] {nameof(AnalyticsScenario.DatasourceId)}
						,[insert_date] {nameof(AnalyticsScenario.InsertDate)}
						,[update_date] {nameof(AnalyticsScenario.UpdateDate)}
						,[datasource_update_date] {nameof(AnalyticsScenario.DatasourceUpdateDate)}
						,[is_deleted] {nameof(AnalyticsScenario.IsDeleted)}
					FROM [mart].[dim_scenario] WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsScenario)))
				.SetReadOnly(true)
				.List<AnalyticsScenario>();
		}

		public AnalyticsScenario Get(Guid scenarioCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT 
						[scenario_id] {nameof(AnalyticsScenario.ScenarioId)}
						,[scenario_code] {nameof(AnalyticsScenario.ScenarioCode)}
						,[scenario_name] {nameof(AnalyticsScenario.ScenarioName)}
						,[default_scenario] {nameof(AnalyticsScenario.DefaultScenario)}
						,[business_unit_id] {nameof(AnalyticsScenario.BusinessUnitId)}
						,[business_unit_code] {nameof(AnalyticsScenario.BusinessUnitCode)}
						,[business_unit_name] {nameof(AnalyticsScenario.BusinessUnitName)}
						,[datasource_id] {nameof(AnalyticsScenario.DatasourceId)}
						,[insert_date] {nameof(AnalyticsScenario.InsertDate)}
						,[update_date] {nameof(AnalyticsScenario.UpdateDate)}
						,[datasource_update_date] {nameof(AnalyticsScenario.DatasourceUpdateDate)}
						,[is_deleted] {nameof(AnalyticsScenario.IsDeleted)}
					FROM [mart].[dim_scenario] WITH (NOLOCK)
					WHERE scenario_code=:{nameof(scenarioCode)}
				")
				.SetParameter(nameof(scenarioCode), scenarioCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsScenario)))
				.UniqueResult<AnalyticsScenario>();
		}
	}
}