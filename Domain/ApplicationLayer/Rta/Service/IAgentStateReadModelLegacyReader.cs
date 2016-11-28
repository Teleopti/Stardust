using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelReader
	{
		IEnumerable<AgentStateReadModel> ReadFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingStatesFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStates);
	}

	public interface IAgentStateReadModelLegacyReader
    {
        IEnumerable<AgentStateReadModel> Read(IEnumerable<IPerson> persons);
        IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds);
	    IEnumerable<AgentStateReadModel> ReadForTeam(Guid teamId);
		
		IEnumerable<AgentStateReadModel> ReadForSites(IEnumerable<Guid> siteIds);
		IEnumerable<AgentStateReadModel> ReadForTeams(IEnumerable<Guid> teamIds);
		IEnumerable<AgentStateReadModel> ReadForSkills(IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadForSitesAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadForTeamsAndSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);

		IEnumerable<AgentStateReadModel> ReadInAlarmForSites(IEnumerable<Guid> siteIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmForTeams(IEnumerable<Guid> teamIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmForSkills(IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmForSitesAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmForTeamsAndSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);

		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSites(IEnumerable<Guid> siteIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForTeams(IEnumerable<Guid> teamIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSkills(IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSitesAndSkill(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForTeamsAndSkill(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds);
	}
}