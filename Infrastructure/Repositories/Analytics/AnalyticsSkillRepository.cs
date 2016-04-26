﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;

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
				@"select 
	                    skillset_id SkillsetId, 
	                    skillset_code SkillsetCode,
	                    skillset_name SkillsetName,
	                    business_unit_id BusinessUnitId,
	                    datasource_id DatasourceId,
	                    insert_date InsertDate,
	                    update_date UpdateDate,
	                    datasource_update_date DatasourceUpdateDate
                    from mart.dim_skillset WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsSkillSet)))
				.SetReadOnly(true)
				.List<AnalyticsSkillSet>();
		}

		public int? SkillSetId(IList<AnalyticsSkill> skills)
		{
			var skillSetCode = string.Join(",", skills.Select(a => a.SkillId).OrderBy(a => a));
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"select skillset_id
                      from mart.dim_skillset WITH (NOLOCK) 
                      where skillset_code=:skillsetCode")
				.SetString("skillsetCode", skillSetCode)
				.UniqueResult<int?>();
		}

		public IEnumerable<AnalyticsSkill> Skills(int businessUnitId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"select 
	                    skill_id SkillId, 
	                    skill_code SkillCode,
	                    skill_name SkillName,
	                    time_zone_id TimeZoneId,
	                    forecast_method_code ForecastMethodCode,
	                    forecast_method_name ForecastMethodName,
	                    business_unit_id BusinessUnitId,
	                    datasource_id DatasourceId,
	                    insert_date InsertDate,
	                    update_date UpdateDate,
	                    datasource_update_date DatasourceUpdateDate,
	                    is_deleted IsDeleted
                    from mart.dim_skill WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsSkill)))
				.SetReadOnly(true)
				.List<AnalyticsSkill>();
		}

		public int AddSkillSet(AnalyticsSkillSet analyticsSkillSet)
		{
			var insertAndUpdateDateTime = DateTime.Now;
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_skillset_insert]
                     @skillset_code=:SkillSetCode
                    ,@skillset_name=:SkillSetName
                    ,@business_unit_id=:BusinessUnitId
                    ,@datasource_id=:DatasourceId
                    ,@insert_date=:InsertDate
                    ,@update_date=:UpdateDate
                    ,@datasource_update_date=:DatasourceUpdateDate")
				.SetString("SkillSetCode", analyticsSkillSet.SkillsetCode)
				.SetString("SkillSetName", analyticsSkillSet.SkillsetName)
				.SetInt32("BusinessUnitId", analyticsSkillSet.BusinessUnitId)
				.SetInt32("DatasourceId", analyticsSkillSet.DatasourceId)
				.SetDateTime("InsertDate", insertAndUpdateDateTime)
				.SetDateTime("UpdateDate", insertAndUpdateDateTime)
				.SetDateTime("DatasourceUpdateDate", analyticsSkillSet.DatasourceUpdateDate);

			return query.ExecuteUpdate();
		}

		public void AddBridgeSkillsetSkill(AnalyticsBridgeSkillsetSkill analyticsBridgeSkillsetSkill)
		{
			var insertAndUpdateDateTime = DateTime.Now;
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_bridge_skillset_skill_insert]
                     @skillset_id=:SkillsetId
                    ,@skill_id=:SkillId
                    ,@business_unit_id=:BusinessUnitId
                    ,@datasource_id=:DatasourceId
                    ,@insert_date=:InsertDate
                    ,@update_date=:UpdateDate
                    ,@datasource_update_date=:DatasourceUpdateDate")
				.SetInt32("SkillsetId", analyticsBridgeSkillsetSkill.SkillsetId)
				.SetInt32("SkillId", analyticsBridgeSkillsetSkill.SkillId)
				.SetInt32("BusinessUnitId", analyticsBridgeSkillsetSkill.BusinessUnitId)
				.SetInt32("DatasourceId", analyticsBridgeSkillsetSkill.DatasourceId)
				.SetDateTime("InsertDate", insertAndUpdateDateTime)
				.SetDateTime("UpdateDate", insertAndUpdateDateTime)
				.SetDateTime("DatasourceUpdateDate", analyticsBridgeSkillsetSkill.DatasourceUpdateDate);

			query.ExecuteUpdate();
		}

		public void AddAgentSkill(int personId, int skillId, bool active, int businessUnitId)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_fact_agent_skill_insert]
					@person_id=:PersonId
					,@skill_id=:SkillId
					,@active=:Active
                    ,@business_unit_id=:BusinessUnitId")
				.SetInt32("PersonId", personId)
				.SetInt32("SkillId", skillId)
				.SetBoolean("Active", active)
				.SetInt32("BusinessUnitId", businessUnitId);

			query.ExecuteUpdate();
		}

		public void DeleteAgentSkillForPersonId(int personId)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_fact_agent_skill_delete]
				@person_id=:PersonId")
				.SetInt32("PersonId", personId);

			query.ExecuteUpdate();
		}

		public IList<AnalyticsFactAgentSkill> GetFactAgentSkillsForPerson(int personId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"SELECT 
						person_id PersonId,
						skill_id SkillId,
						has_skill HasSkill,
						active Active,
						business_unit_id BusinessUnitId,
						datasource_id DatasourceId
                    FROM mart.fact_agent_skill WITH (NOLOCK)
					WHERE person_id=:PersonId")
				.SetInt32("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsFactAgentSkill)))
				.SetReadOnly(true)
				.List<AnalyticsFactAgentSkill>();
		}

		public void AddOrUpdateSkill(AnalyticsSkill analyticsSkill)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_skill_add_or_update]
					@skill_code=:SkillCode,
					@skill_name=:SkillName,
					@time_zone_id=:TimeZoneId,
					@forecast_method_code=:ForecastMethodCode,
					@forecast_method_name=:ForecastMethodName,
					@business_unit_id=:BusinessUnitId,
					@datasource_update_date=:DatasourceUpdateDate,
					@is_deleted=:IsDeleted")
				.SetGuid("SkillCode", analyticsSkill.SkillCode)
				.SetString("SkillName", analyticsSkill.SkillName)
				.SetInt32("TimeZoneId", analyticsSkill.TimeZoneId)
				.SetGuid("ForecastMethodCode", analyticsSkill.ForecastMethodCode)
				.SetString("ForecastMethodName", analyticsSkill.ForecastMethodName)
				.SetInt32("BusinessUnitId", analyticsSkill.BusinessUnitId)
				.SetDateTime("DatasourceUpdateDate", analyticsSkill.DatasourceUpdateDate)
				.SetBoolean("IsDeleted", analyticsSkill.IsDeleted)
				;
			query.ExecuteUpdate();
		}
	}
}