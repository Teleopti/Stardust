using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

using Teleopti.Wfm.Adherence.ApplicationLayer.Infrastructure;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor.Infrastructure;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeOrganizationReader : IOrganizationReader
	{
		private readonly INow _now;
		private readonly FakeAgentStateReadModelPersister _agentStateReadModels;
		private readonly IGroupingReadOnlyRepository _groupings;
		private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;

		public FakeOrganizationReader(
			INow now,
			FakeAgentStateReadModelPersister agentStateReadModels,
			IGroupingReadOnlyRepository groupings,
			HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId
		)
		{
			_now = now;
			_agentStateReadModels = agentStateReadModels;
			_groupings = groupings;
			_hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
		}

		public IEnumerable<OrganizationSiteModel> Read()
		{
			return read(s => true);
		}

		public IEnumerable<OrganizationSiteModel> Read(IEnumerable<Guid> skillIds)
		{
			var agentsWithSkills = this.agentsWithSkills(skillIds);
			return read(x => agentsWithSkills.Contains(x.PersonId));
		}

		private IEnumerable<Guid> agentsWithSkills(IEnumerable<Guid> skillIds)
		{
			return _groupings
				.DetailsForGroup(_hardcodedSkillGroupingPageId.GetGuid(), new DateOnly(_now.UtcDateTime()))
				.Where(x => skillIds.Contains(x.GroupId))
				.Select(x => x.PersonId)
				.Distinct()
				.ToArray();
		}

		private IEnumerable<OrganizationSiteModel> read(Predicate<AgentStateReadModel> these)
		{
			return _agentStateReadModels
				.Models
				.Where(x => these(x))
				.GroupBy(x => new {x.SiteId, x.SiteName})
				.Select(x => new OrganizationSiteModel
				{
					SiteId = x.Key.SiteId.Value,
					SiteName = x.Key.SiteName,
					Teams = x.Select(t =>
						new OrganizationTeamModel {TeamId = t.TeamId.Value, TeamName = t.TeamName})
				})
				.ToArray();
		}
	}
}