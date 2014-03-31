using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class NumberOfAgentsInSiteReader : INumberOfAgentsInSiteReader
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;
		private const string sqlQuery = @"
SELECT
	a.Site as 'SiteId',
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
and a.Site in (:sites)
group by a.Site";


		public NumberOfAgentsInSiteReader(ICurrentUnitOfWork currentUnitOfWork, INow now)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
		}

		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<ISite> sites)
		{
			var ret = new Dictionary<Guid, int>();
			var queryResult = _currentUnitOfWork.Session().CreateSQLQuery(sqlQuery)
				.SetParameterList("sites", sites.Select(x => x.Id.Value))
				.SetDateTime("now", _now.UtcDateTime())
				.List();

			foreach (var site in sites)
			{
				ret[site.Id.Value] = 0;
			}

			foreach (var resItemArray in queryResult)
			{
				var resItem = (IList) resItemArray;
				var siteId = (Guid)resItem[0];
				var noOf = (int)resItem[1];
				ret[siteId] = noOf;
			}

			return ret;
		}
	}
}