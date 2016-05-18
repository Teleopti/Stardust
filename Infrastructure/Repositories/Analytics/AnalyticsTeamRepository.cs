using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsTeamRepository : IAnalyticsTeamRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsTeamRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IList<AnalyticTeam> GetTeams()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(@"select 
					team_id TeamId
					,team_code TeamCode
					from mart.dim_team WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean<AnalyticTeam>())
				.List<AnalyticTeam>();
		}

		public int GetOrCreate(Guid teamCode, int siteId, string teamName, int businessUnitId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(@"mart.etl_dim_team_id_get @team_code=:teamCode,@team_name=:teamName, @site_id=:siteId , @business_unit_id=:businessUnitId")
				.SetGuid("teamCode", teamCode)
				.SetString("teamName", teamName)
				.SetInt32("siteId", siteId)
				.SetInt32("businessUnitId", businessUnitId)
				.UniqueResult<int>();
		}
	}
}