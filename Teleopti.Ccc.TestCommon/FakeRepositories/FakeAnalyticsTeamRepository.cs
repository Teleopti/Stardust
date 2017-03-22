using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsTeamRepository : IAnalyticsTeamRepository
	{
		private readonly IList<AnalyticTeam> _analyticTeams = new List<AnalyticTeam>();

		public IList<AnalyticTeam> GetTeams()
		{
			return _analyticTeams;
		}

		public int GetOrCreate(Guid teamCode, int siteId, string teamName, int businessUnitId)
		{
			var team = _analyticTeams.FirstOrDefault(t => t.TeamCode == teamCode);
			if (team == null)
			{
				team = new AnalyticTeam{TeamCode = teamCode, TeamId = 456};
				_analyticTeams.Add(team);
			}
			return team.TeamId;
		}

		public void Has(AnalyticTeam team)
		{
			_analyticTeams.Add(team);
		}
	}
}