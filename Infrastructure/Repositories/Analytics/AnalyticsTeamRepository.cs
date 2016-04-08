using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsTeamRepository : IAnalyticsTeamRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public AnalyticsTeamRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public IList<AnalyticTeam> GetTeams()
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(@"select 
						team_id TeamId
						,team_code TeamCode
						from mart.dim_team WITH (NOLOCK)")
					.SetResultTransformer(Transformers.AliasToBean<AnalyticTeam>())
					.List<AnalyticTeam>();
			}
		}

		public int GetOrCreate(Guid teamCode, int siteId, string teamName, int businessUnitId)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(@"mart.etl_dim_team_id_get @team_code=:teamCode,@team_name=:teamName, @site_id=:siteId , @business_unit_id=:businessUnitId")
					.SetGuid("teamCode", teamCode)
					.SetString("teamName", teamName)
					.SetInt32("siteId", siteId)
					.SetInt32("businessUnitId", businessUnitId)
					.UniqueResult<int>();
			}
		}
	}
}