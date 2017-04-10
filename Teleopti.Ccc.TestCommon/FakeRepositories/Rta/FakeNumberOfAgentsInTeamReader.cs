using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeNumberOfAgentsInTeamReader : INumberOfAgentsInTeamReader
	{
		private readonly IPersonRepository _persons;
		private readonly INow _now;
		private readonly IGroupingReadOnlyRepository _groupings;
		private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;

		public FakeNumberOfAgentsInTeamReader(IPersonRepository persons, INow now, HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId, IGroupingReadOnlyRepository groupings)
		{
			_persons = persons;
			_now = now;
			_hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
			_groupings = groupings;
		}

		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> teams)
		{
			return
				(from person in _persons.LoadAll()
					from team in teams
					let today = new DateOnly(_now.UtcDateTime())
					let myTeam = person.MyTeam(today)
					where myTeam?.Id != null && myTeam.Id.Value == team
					group person by myTeam.Id.Value
					into g
				 select g)
				.ToDictionary(g => g.Key, g => g.Count());
		}

		public IDictionary<Guid, int> ForSkills(IEnumerable<Guid> teams, IEnumerable<Guid> skillIds)
		{
			return
				(from person in _persons.LoadAll()
				 from agentSkill in _groupings.DetailsForGroup(_hardcodedSkillGroupingPageId.GetGuid(), new DateOnly(_now.UtcDateTime()))
				 from skill in skillIds
				 from team in teams
				 let today = new DateOnly(_now.UtcDateTime())
				 let myTeam = person.MyTeam(today)
				 where myTeam?.Id != null && myTeam.Id.Value == team &&
				 agentSkill.GroupId == skill && agentSkill.PersonId == person.Id
				 group person by myTeam.Id.Value
					into g
				 select g)
				.ToDictionary(g => g.Key, g => g.Count());

		}
	}
}