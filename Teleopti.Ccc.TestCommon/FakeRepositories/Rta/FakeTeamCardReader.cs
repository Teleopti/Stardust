using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

using Teleopti.Wfm.Adherence.ApplicationLayer.Infrastructure;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeTeamCardReader : ITeamCardReader
    {
	    private readonly INow _now;
	    private readonly FakeAgentStateReadModelPersister _agentStateReadModels;
	    private readonly IGroupingReadOnlyRepository _groupings;
	    private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;
		
	    public FakeTeamCardReader(
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

		public IEnumerable<TeamCardModel> Read()
		{
			return read(x => true);
		}

		public IEnumerable<TeamCardModel> Read(IEnumerable<Guid> skillIds)
	    {
		    var agentsWithSkills = this.agentsWithSkills(skillIds);
		    return read(x => agentsWithSkills.Contains(x.PersonId));
	    }

		public IEnumerable<TeamCardModel> Read(Guid siteId)
		{
			return read(x => x.SiteId == siteId);
		}

	    public IEnumerable<TeamCardModel> Read(Guid siteId, IEnumerable<Guid> skillIds)
	    {
		    var agentsWithSkills = this.agentsWithSkills(skillIds);
		    return read(x => x.SiteId == siteId && agentsWithSkills.Contains(x.PersonId));
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

	    private IEnumerable<TeamCardModel> read(Predicate<AgentStateReadModel> these)
	    {
		    return _agentStateReadModels
			    .Models
			    .Where(x => these(x))
			    .GroupBy(x => x.TeamId)
			    .Select(group => new TeamCardModel
			    {
				    BusinessUnitId = @group.First().BusinessUnitId.GetValueOrDefault(),
				    SiteId = @group.First().SiteId.Value,
				    SiteName = @group.First().SiteName,
					TeamId = @group.Key.Value,
					TeamName = @group.First().TeamName,
					InAlarmCount = @group.Count(s => s.IsRuleAlarm && s.AlarmStartTime <= _now.UtcDateTime()),
					AgentsCount = @group.Count()
			    })
			    .ToArray();
	    }
    }
}