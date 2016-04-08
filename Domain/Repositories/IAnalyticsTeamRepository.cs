using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsTeamRepository
	{
		IList<AnalyticTeam> GetTeams();
		int GetOrCreate(Guid teamCode, int siteId, string teamName, int businessUnitId);
	}
}