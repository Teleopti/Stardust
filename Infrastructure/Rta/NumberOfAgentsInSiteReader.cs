﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
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
inner join person p on 
a.parent = p.id
where a.is_current=1
and a.Site in (:sites)
and (p.TerminalDate is null or p.TerminalDate > :now)
group by a.Site";


		public NumberOfAgentsInSiteReader(ICurrentUnitOfWork currentUnitOfWork, INow now)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
		}

		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> sites)
		{
			var ret = new Dictionary<Guid, int>();
			var queryResult = _currentUnitOfWork.Session().CreateSQLQuery(sqlQuery)
				.SetParameterList("sites", sites)
				.SetDateTime("now", _now.UtcDateTime())
				.List();

			foreach (var site in sites)
			{
				ret[site] = 0;
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

		public IDictionary<Guid, int> ForSkills(IEnumerable<Guid> sites, IEnumerable<Guid> skillIds)
		{
			return null;
		}
	}
}