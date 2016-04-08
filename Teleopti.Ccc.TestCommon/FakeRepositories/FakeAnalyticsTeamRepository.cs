using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsTeamRepository : IAnalyticsTeamRepository
	{
		public IList<AnalyticTeam> GetTeams()
		{
			throw new NotImplementedException();
		}

		public int GetOrCreate(Guid teamCode, int siteId, string teamName, int businessUnitId)
		{
			return 456;
		}
	}
}