using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeNumberOfAgentsInTeamReader : INumberOfAgentsInTeamReader
	{
		private readonly List<agentsInTeam> _data = new List<agentsInTeam>();

		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> teams)
		{
			return
				(from d in _data
				 from t in teams
				 where d.TeamId == t
				 select d)
				 .ToDictionary(x => x.TeamId, y => y.NumberOfAgents);
		}

		public IDictionary<Guid, int> ForSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			return
				(from d in _data
					from t in teamIds
					from s in skillIds
					where d.TeamId == t && d.SkillId == s
					select d).ToDictionary(x => x.TeamId, y => y.NumberOfAgents);
		}

		public void Has(Guid teamId, int numberOfAgents)
		{
			Has(teamId, Guid.Empty, numberOfAgents);
		}

		public void Has(Guid teamId, Guid skillId, int numberOfAgents)
		{
			_data.Add(new agentsInTeam
			{
				TeamId = teamId,
				SkillId = skillId,
				NumberOfAgents = numberOfAgents
			});
		}


		private class agentsInTeam
		{
			public Guid TeamId { get; set; }
			public Guid SkillId { get; set; }
			public int NumberOfAgents { get; set; }
		}
	}
}