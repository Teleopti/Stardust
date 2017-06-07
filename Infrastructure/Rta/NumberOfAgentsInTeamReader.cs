using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class NumberOfAgentsInTeamReader : INumberOfAgentsInTeamReader
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;
		private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;

		public NumberOfAgentsInTeamReader(ICurrentUnitOfWork currentUnitOfWork, INow now, HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
			_hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
		}

		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> teams)
		{
			if (teams.IsEmpty()) return new Dictionary<Guid, int>();

			var models =
				_currentUnitOfWork.Session().CreateSQLQuery(@"
SELECT
	Team as 'TeamId',
	count(Parent) as 'AgentsCount'
FROM dbo.v_PersonPeriodTeamSiteBu WITH(NOEXPAND)
WHERE :now BETWEEN StartDate AND EndDate 
and Team in (:teams)
group by Team
")
					.SetDateTime("now", _now.UtcDateTime().Date)
					.SetParameterList("teams", teams)
					.SetResultTransformer(Transformers.AliasToBean(typeof(teamViewModel)))
					.List()
					.Cast<teamViewModel>();

			var initializedSites =
				from teamId in teams
				where !models.Select(x => x.TeamId).Contains(teamId)
				select new teamViewModel { TeamId = teamId, AgentsCount = 0 };

			return initializedSites.Concat(models).ToDictionary(x => x.TeamId, y => y.AgentsCount);
		}

		public IDictionary<Guid, int> ForSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			var models = _currentUnitOfWork.Session()
				.CreateSQLQuery(@"
SELECT
	pp.Team as 'TeamId',
	count(DISTINCT pp.Parent) as 'AgentsCount'
FROM dbo.v_PersonPeriodTeamSiteBu pp WITH(NOEXPAND)

INNER JOIN ReadModel.GroupingReadOnly AS g
ON pp.Parent = g.PersonId					
WHERE g.GroupId IN (:skillIds)
AND g.PageId = :skillGroupingPageId
AND :now BETWEEN g.StartDate AND g.EndDate
					
AND pp.Team in (:teams)
GROUP BY pp.Team
")
				.SetDateTime("now", _now.UtcDateTime().Date)
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetParameterList("skillIds", skillIds)
				.SetParameterList("teams", teamIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof(teamViewModel)))
				.List()
				.Cast<teamViewModel>();

			return models.ToDictionary(x => x.TeamId, y => y.AgentsCount);
		}

		private class teamViewModel
		{
			public Guid TeamId { get; set; }
			public int AgentsCount { get; set; }
		}
	}
}