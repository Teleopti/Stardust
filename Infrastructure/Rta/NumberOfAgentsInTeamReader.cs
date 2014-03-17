using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class NumberOfAgentsInTeamReader
	{
		private readonly CurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;
		private const string sqlQuery = @"
SELECT
	a.Team as 'TeamId',
	count(a.Parent) as 'NumberOfAgents'
FROM
(
	SELECT
	pp.StartDate,
	pp.Parent,
	pp.PersonPeriod,
	pp.BusinessUnit,
	pp.Site,
	pp.Team,
	ROW_NUMBER()OVER(PARTITION BY pp.Parent ORDER BY pp.StartDate DESC) as is_current
	FROM dbo.v_PersonPeriodTeamSiteBu pp WITH(NOEXPAND)
	WHERE pp.StartDate <= :now 
) a
where a.is_current=1
and a.Team in (:teams)
group by a.Team";


		public NumberOfAgentsInTeamReader(CurrentUnitOfWork currentUnitOfWork, INow now)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
		}

		public IDictionary<Guid, int> FetchNumberOfAgents(ITeam[] teams)
		{
			var ret = new Dictionary<Guid, int>();
			var queryResult = _currentUnitOfWork.Session().CreateSQLQuery(sqlQuery)
				.SetParameterList("teams", teams.Select(x => x.Id.Value))
				.SetDateTime("now", _now.UtcDateTime())
				.List();

			foreach (var team in teams)
			{
				ret[team.Id.Value] = 0;
			}

			foreach (var resItemArray in queryResult)
			{
				var resItem = (IList)resItemArray;
				var teamId = (Guid)resItem[0];
				var noOf = (int)resItem[1];
				ret[teamId] = noOf;
			}

			return ret;
		}
	}
}