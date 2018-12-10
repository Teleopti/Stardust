using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsSkillRepository : IAnalyticsSkillRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsSkillRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IList<AnalyticsSkillSet> SkillSets()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
	                    skillset_id {nameof(AnalyticsSkillSet.SkillsetId)}, 
	                    skillset_code {nameof(AnalyticsSkillSet.SkillsetCode)},
	                    skillset_name {nameof(AnalyticsSkillSet.SkillsetName)},
	                    business_unit_id {nameof(AnalyticsSkillSet.BusinessUnitId)},
	                    datasource_id {nameof(AnalyticsSkillSet.DatasourceId)},
	                    insert_date {nameof(AnalyticsSkillSet.InsertDate)},
	                    update_date {nameof(AnalyticsSkillSet.UpdateDate)},
	                    datasource_update_date {nameof(AnalyticsSkillSet.DatasourceUpdateDate)}
                    from mart.dim_skillset WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsSkillSet)))
				.SetReadOnly(true)
				.List<AnalyticsSkillSet>();
		}

		public int? SkillSetId(IList<AnalyticsSkill> skills)
		{
			var skillSetCode = string.Join(",", skills.Select(a => a.SkillId).OrderBy(a => a));
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select skillset_id
                      from mart.dim_skillset WITH (NOLOCK) 
                      where skillset_code=:{nameof(skillSetCode)}")
				.SetString(nameof(skillSetCode), skillSetCode)
				.UniqueResult<int?>();
		}

		public IEnumerable<AnalyticsSkill> Skills(int businessUnitId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
	                    skill_id {nameof(AnalyticsSkill.SkillId)}, 
	                    skill_code {nameof(AnalyticsSkill.SkillCode)},
	                    skill_name {nameof(AnalyticsSkill.SkillName)},
	                    time_zone_id {nameof(AnalyticsSkill.TimeZoneId)},
	                    forecast_method_code {nameof(AnalyticsSkill.ForecastMethodCode)},
	                    forecast_method_name {nameof(AnalyticsSkill.ForecastMethodName)},
	                    business_unit_id {nameof(AnalyticsSkill.BusinessUnitId)},
	                    datasource_id {nameof(AnalyticsSkill.DatasourceId)},
	                    insert_date {nameof(AnalyticsSkill.InsertDate)},
	                    update_date {nameof(AnalyticsSkill.UpdateDate)},
	                    datasource_update_date {nameof(AnalyticsSkill.DatasourceUpdateDate)},
	                    is_deleted {nameof(AnalyticsSkill.IsDeleted)}
                    from mart.dim_skill WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsSkill)))
				.SetReadOnly(true)
				.List<AnalyticsSkill>();
		}

		public int AddSkillSet(AnalyticsSkillSet analyticsSkillSet)
		{
			var insertAndUpdateDateTime = DateTime.Now;
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_skillset_insert]
                     @skillset_code=:{nameof(AnalyticsSkillSet.SkillsetCode)}
                    ,@skillset_name=:{nameof(AnalyticsSkillSet.SkillsetName)}
                    ,@business_unit_id=:{nameof(AnalyticsSkillSet.BusinessUnitId)}
                    ,@datasource_id=:{nameof(AnalyticsSkillSet.DatasourceId)}
                    ,@insert_date=:{nameof(AnalyticsSkillSet.InsertDate)}
                    ,@update_date=:{nameof(AnalyticsSkillSet.UpdateDate)}
                    ,@datasource_update_date=:{nameof(AnalyticsSkillSet.DatasourceUpdateDate)}")
				.SetString(nameof(AnalyticsSkillSet.SkillsetCode), analyticsSkillSet.SkillsetCode)
				.SetString(nameof(AnalyticsSkillSet.SkillsetName), analyticsSkillSet.SkillsetName)
				.SetInt32(nameof(AnalyticsSkillSet.BusinessUnitId), analyticsSkillSet.BusinessUnitId)
				.SetInt32(nameof(AnalyticsSkillSet.DatasourceId), analyticsSkillSet.DatasourceId)
				.SetDateTime(nameof(AnalyticsSkillSet.InsertDate), insertAndUpdateDateTime)
				.SetDateTime(nameof(AnalyticsSkillSet.UpdateDate), insertAndUpdateDateTime)
				.SetDateTime(nameof(AnalyticsSkillSet.DatasourceUpdateDate), analyticsSkillSet.DatasourceUpdateDate);

			return query.ExecuteUpdate();
		}

		public void AddBridgeSkillsetSkill(AnalyticsBridgeSkillsetSkill analyticsBridgeSkillsetSkill)
		{
			var insertAndUpdateDateTime = DateTime.Now;
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_bridge_skillset_skill_insert]
                     @skillset_id=:{nameof(AnalyticsBridgeSkillsetSkill.SkillsetId)}
                    ,@skill_id=:{nameof(AnalyticsBridgeSkillsetSkill.SkillId)}
                    ,@business_unit_id=:{nameof(AnalyticsBridgeSkillsetSkill.BusinessUnitId)}
                    ,@datasource_id=:{nameof(AnalyticsBridgeSkillsetSkill.DatasourceId)}
                    ,@insert_date=:{nameof(AnalyticsBridgeSkillsetSkill.InsertDate)}
                    ,@update_date=:{nameof(AnalyticsBridgeSkillsetSkill.UpdateDate)}
                    ,@datasource_update_date=:{nameof(AnalyticsBridgeSkillsetSkill.DatasourceUpdateDate)}")
				.SetInt32(nameof(AnalyticsBridgeSkillsetSkill.SkillsetId), analyticsBridgeSkillsetSkill.SkillsetId)
				.SetInt32(nameof(AnalyticsBridgeSkillsetSkill.SkillId), analyticsBridgeSkillsetSkill.SkillId)
				.SetInt32(nameof(AnalyticsBridgeSkillsetSkill.BusinessUnitId), analyticsBridgeSkillsetSkill.BusinessUnitId)
				.SetInt32(nameof(AnalyticsBridgeSkillsetSkill.DatasourceId), analyticsBridgeSkillsetSkill.DatasourceId)
				.SetDateTime(nameof(AnalyticsBridgeSkillsetSkill.InsertDate), insertAndUpdateDateTime)
				.SetDateTime(nameof(AnalyticsBridgeSkillsetSkill.UpdateDate), insertAndUpdateDateTime)
				.SetDateTime(nameof(AnalyticsBridgeSkillsetSkill.DatasourceUpdateDate), analyticsBridgeSkillsetSkill.DatasourceUpdateDate);

			query.ExecuteUpdate();
		}

		public void AddAgentSkill(int personId, int skillId, bool active, int businessUnitId)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_fact_agent_skill_insert]
					@person_id=:{nameof(personId)}
					,@skill_id=:{nameof(skillId)}
					,@active=:{nameof(active)}
                    ,@business_unit_id=:{nameof(businessUnitId)}")
				.SetInt32(nameof(personId), personId)
				.SetInt32(nameof(skillId), skillId)
				.SetBoolean(nameof(active), active)
				.SetInt32(nameof(businessUnitId), businessUnitId);

			query.ExecuteUpdate();
		}

		public void DeleteAgentSkillForPersonId(int personId)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_fact_agent_skill_delete]
				@person_id=:{nameof(personId)}")
				.SetInt32(nameof(personId), personId);

			query.ExecuteUpdate();
		}

		public void AddOrUpdateSkill(AnalyticsSkill analyticsSkill)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_skill_add_or_update]
					@skill_code=:{nameof(AnalyticsSkill.SkillCode)},
					@skill_name=:{nameof(AnalyticsSkill.SkillName)},
					@time_zone_id=:{nameof(AnalyticsSkill.TimeZoneId)},
					@forecast_method_code=:{nameof(AnalyticsSkill.ForecastMethodCode)},
					@forecast_method_name=:{nameof(AnalyticsSkill.ForecastMethodName)},
					@business_unit_id=:{nameof(AnalyticsSkill.BusinessUnitId)},
					@datasource_update_date=:{nameof(AnalyticsSkill.DatasourceUpdateDate)},
					@is_deleted=:{nameof(AnalyticsSkill.IsDeleted)}")
				.SetGuid(nameof(AnalyticsSkill.SkillCode), analyticsSkill.SkillCode)
				.SetString(nameof(AnalyticsSkill.SkillName), analyticsSkill.SkillName)
				.SetInt32(nameof(AnalyticsSkill.TimeZoneId), analyticsSkill.TimeZoneId)
				.SetGuid(nameof(AnalyticsSkill.ForecastMethodCode), analyticsSkill.ForecastMethodCode)
				.SetString(nameof(AnalyticsSkill.ForecastMethodName), analyticsSkill.ForecastMethodName)
				.SetInt32(nameof(AnalyticsSkill.BusinessUnitId), analyticsSkill.BusinessUnitId)
				.SetDateTime(nameof(AnalyticsSkill.DatasourceUpdateDate), analyticsSkill.DatasourceUpdateDate)
				.SetBoolean(nameof(AnalyticsSkill.IsDeleted), analyticsSkill.IsDeleted)
				;
			query.ExecuteUpdate();
		}
	}
}