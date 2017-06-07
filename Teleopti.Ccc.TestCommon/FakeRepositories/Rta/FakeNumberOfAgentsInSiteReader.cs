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
	public class FakeNumberOfAgentsInSiteReader : INumberOfAgentsInSiteReader
	{
		private readonly IPersonRepository _persons;
		private readonly INow _now;
		private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;
		private readonly IGroupingReadOnlyRepository _groupings;
		private readonly FakeAgentStateReadModelPersister _agentStates;

		public FakeNumberOfAgentsInSiteReader(
			IPersonRepository persons, 
			INow now, 
			HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId, 
			IGroupingReadOnlyRepository groupings, FakeAgentStateReadModelPersister agentStates)
		{
			_persons = persons;
			_now = now;
			_hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
			_groupings = groupings;
			_agentStates = agentStates;
		}
		public IDictionary<Guid, int> Read(IEnumerable<Guid> siteIds)
		{
			return 
				(from siteId in siteIds
					from model in _agentStates.Models
				 where siteId == model.SiteId
					group model by model.SiteId.Value into g
				 select g
					).ToDictionary(x => x.Key, y => y.Count());
		}

		public IDictionary<Guid, int> Read(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds)
		{

			return
				(from person in _persons.LoadAll()
				 let today = new DateOnly(_now.UtcDateTime())
				 from agentSkill in _groupings.DetailsForGroup(_hardcodedSkillGroupingPageId.GetGuid(), today)
				 from skill in skillIds
				 from siteId in siteIds
				 let site = person.MyTeam(today)?.Site
				 where site?.Id != null && site.Id.Value == siteId &&
				 agentSkill.GroupId == skill && agentSkill.PersonId == person.Id
				 group person by site.Id.Value
					into g
				 select g)
				.ToDictionary(g => g.Key, g => g.Count());
		}
	}
}
