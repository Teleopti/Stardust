using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class NumberOfAgentsInTeamReader : INumberOfAgentsInTeamReader
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;

		public NumberOfAgentsInTeamReader(ICurrentUnitOfWork currentUnitOfWork, INow now)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
		}

		private const string agentsForTeams = @"
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
inner join person p on 
a.parent = p.id
where a.is_current=1
and a.Team in (:teams)
and (p.TerminalDate is null or p.TerminalDate > :now)
and p.isDeleted = 0
group by a.Team";
		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> teams)
		{
			if (teams.IsEmpty()) return new Dictionary<Guid, int>();

			var models =
				_currentUnitOfWork.Session().CreateSQLQuery(agentsForTeams)
					.SetDateTime("now", _now.UtcDateTime())
					.SetParameterList("teams", teams)
					.SetResultTransformer(Transformers.AliasToBean(typeof(teamViewModel)))
					.List()
					.Cast<teamViewModel>();

			var initializedSites =
				from teamId in teams
				where !models.Select(x => x.TeamId).Contains(teamId)
				select new teamViewModel { TeamId = teamId, NumberOfAgents = 0 };

			return initializedSites.Concat(models).ToDictionary(x => x.TeamId, y => y.NumberOfAgents);
		}

		private const string agentsForSkillQuery = @"
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
inner join person p on 
a.parent = p.id


INNER JOIN ReadModel.GroupingReadOnly AS g
ON p.Id = g.PersonId					
WHERE g.GroupId IN (:skillIds)
AND g.PageId = :skillGroupingPageId


and a.is_current=1
and a.Team in (:teams)
and (p.TerminalDate is null or p.TerminalDate > :now)
and p.isDeleted = 0
group by a.Team
";
		public IDictionary<Guid, int> ForSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			var models = _currentUnitOfWork.Session()
				.CreateSQLQuery(agentsForSkillQuery)
				.SetDateTime("now", _now.UtcDateTime())
				.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Get)
				.SetParameterList("skillIds", skillIds)
				.SetParameterList("teams", teamIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof(teamViewModel)))
				.List()
				.Cast<teamViewModel>();

			return models.ToDictionary(x => x.TeamId, y => y.NumberOfAgents);
		}

		private class teamViewModel
		{
			public Guid TeamId { get; set; }
			public int NumberOfAgents { get; set; }
		}
	}
}