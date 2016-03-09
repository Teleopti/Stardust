using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsSkillRepository : IAnalyticsSkillRepository
	{
		public IList<AnalyticsSkillSet> SkillSets()
		{
			using (var uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					@"select 
	                    skillset_id SkillsetId, 
	                    skillset_code SkillsetCode,
	                    skillset_name SkillsetName,
	                    business_unit_id BusinessUnitId,
	                    datasource_id DatasourceId,
	                    insert_date InsertDate,
	                    update_date UpdateDate,
	                    datasource_update_date DatasourceUpdateDate,
                    from mart.dim_skillset WITH (NOLOCK)")
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsSkillSet)))
					.SetReadOnly(true)
					.List<AnalyticsSkillSet>();
			}
		}

		public int? SkillSetId(IList<AnalyticsSkill> skills)
		{
			using (var uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				var skillSetCode = string.Join(",", skills.Select(a => a.SkillId).OrderBy(a => a));
				return uow.Session().CreateSQLQuery(
					@"select skillset_id
                      from mart.dim_skillset WITH (NOLOCK) 
                      where skillset_code=:skillsetCode")
					.SetString("skillsetCode", skillSetCode)
					.UniqueResult<int?>();
			}
		}

		public IList<AnalyticsSkill> Skills(int businessUnitId)
		{
			using (var uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
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
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsSkill)))
					.SetReadOnly(true)
					.List<AnalyticsSkill>();
			}
		}

		public int AddSkillSet(AnalyticsSkillSet analyticsSkillSet)
		{
			using (var uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				var insertAndUpdateDateTime = DateTime.Now;
				var query = uow.Session().CreateSQLQuery(
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
		}

		public void AddBridgeSkillsetSkill(AnalyticsBridgeSkillsetSkill analyticsBridgeSkillsetSkill)
		{
			throw new NotImplementedException();
		}

		public int AddSkillSet(List<AnalyticsSkill> skills)
		{
			var skillSetCode = string.Join(",", skills.OrderBy(a => a.SkillId).Select(a => a.SkillId));
			var skillSetName = string.Join(",", skills.OrderBy(a => a.SkillId).Select(a => a.SkillName));
			
			var newSkillSet = new AnalyticsSkillSet
			{
				SkillsetCode = skillSetCode,
				SkillsetName = skillSetName,
				BusinessUnitId = skills.First().BusinessUnitId,
				DatasourceId = skills.First().DatasourceId,
				DatasourceUpdateDate = skills.Max(a => a.DatasourceUpdateDate)
			};

			return AddSkillSet(newSkillSet);
		}

		private IAnalyticsUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = (ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity;
			return identity.DataSource.Analytics;
		}
	}
}