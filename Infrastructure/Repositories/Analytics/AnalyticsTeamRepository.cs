using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"select 
					team_id {nameof(AnalyticTeam.TeamId)}
					,team_code {nameof(AnalyticTeam.TeamCode)}
					from mart.dim_team WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean<AnalyticTeam>())
				.List<AnalyticTeam>();
		}

		public int GetOrCreate(Guid teamCode, int siteId, string teamName, int businessUnitId)
		{
			return
				_analyticsUnitOfWork.Current()
					.Session()
					.CreateSQLQuery(
						$@"mart.etl_dim_team_id_get @team_code=:{nameof(teamCode)},@team_name=:{nameof(teamName)}, @site_id=:{nameof(
							siteId)} , @business_unit_id=:{nameof(businessUnitId)}")
					.SetGuid(nameof(teamCode), teamCode)
					.SetString(nameof(teamName), teamName)
					.SetInt32(nameof(siteId), siteId)
					.SetInt32(nameof(businessUnitId), businessUnitId)
					.UniqueResult<int>();
		}
	}
}