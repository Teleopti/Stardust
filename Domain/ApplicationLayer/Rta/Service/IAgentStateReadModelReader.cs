using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelReader
    {
        IEnumerable<AgentStateReadModel> Read(IEnumerable<IPerson> persons);
        IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds);
	    IEnumerable<AgentStateReadModel> ReadForTeam(Guid teamId);

		IEnumerable<AgentStateReadModel> ReadForSites(IEnumerable<Guid> siteIds);
		IEnumerable<AgentStateReadModel> ReadForTeams(IEnumerable<Guid> teamIds);
		IEnumerable<AgentStateReadModel> ReadForSkills(IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadForSitesAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadForTeamsAndSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);

		IEnumerable<AgentStateReadModel> ReadInAlarmsForSites(IEnumerable<Guid> siteIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmsForTeams(IEnumerable<Guid> teamIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmsForSkills(IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmsForSitesAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmsForTeamsAndSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);

		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSites(IEnumerable<Guid> siteIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForTeams(IEnumerable<Guid> teamIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSkills(IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSitesAndSkill(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForTeamsAndSkill(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds);
	}
}