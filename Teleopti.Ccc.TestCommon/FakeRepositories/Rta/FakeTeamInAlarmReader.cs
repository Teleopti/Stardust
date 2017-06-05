using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
    public class FakeTeamInAlarmReader : ITeamInAlarmReader
    {
	    private readonly INow _now;
	    private readonly FakeAgentStateReadModelPersister _agentStateReadModels;
	    private readonly IGroupingReadOnlyRepository _groupings;
	    private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;
		
	    public FakeTeamInAlarmReader(INow now, 
			FakeAgentStateReadModelPersister agentStateReadModels, 
			IGroupingReadOnlyRepository groupings,
			HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId)
	    {
		    _now = now;
		    _agentStateReadModels = agentStateReadModels;
		    _groupings = groupings;
		    _hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
	    }
		
		public IEnumerable<TeamInAlarmModel> Read(Guid siteId)
		{
			return _agentStateReadModels
				.Models
				.Where(x =>
					x.SiteId == siteId &&
					x.IsRuleAlarm &&
					x.AlarmStartTime <= _now.UtcDateTime())
				.GroupBy(x => x.TeamId)
				.Select(x => new TeamInAlarmModel
				{
					SiteId = siteId,
					TeamId = x.Key.Value,
					Count = x.Count()
				});
		}

	    public IEnumerable<TeamInAlarmModel> Read(Guid siteId, IEnumerable<Guid> skillIds)
	    {
			return
				from agentState in _agentStateReadModels.Models
				from agentSkill in _groupings.DetailsForGroup(_hardcodedSkillGroupingPageId.GetGuid(), new DateOnly(_now.UtcDateTime()))
				from skill in skillIds
				where agentSkill.GroupId == skill &&
					  agentSkill.PersonId == agentState.PersonId &&
					  agentState.SiteId == siteId &&
					  _now.UtcDateTime() >= agentState.AlarmStartTime &&
					  agentState.IsRuleAlarm
				group agentState by agentState.TeamId
				into g
				select new TeamInAlarmModel
				{
					SiteId = siteId,
					TeamId = g.Key.GetValueOrDefault(),
					Count = g.Select(x => x.PersonId).Distinct().Count()
				};
			
	    }
	}
}