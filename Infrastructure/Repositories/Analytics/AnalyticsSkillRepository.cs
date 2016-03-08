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

		private IAnalyticsUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = (ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity;
			return identity.DataSource.Analytics;
		}
	}
}