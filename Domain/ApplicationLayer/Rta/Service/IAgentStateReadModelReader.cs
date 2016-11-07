using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelReader
    {
        IEnumerable<AgentStateReadModel> Load(IEnumerable<IPerson> persons);
        IEnumerable<AgentStateReadModel> Load(IEnumerable<Guid> personIds);
	    IEnumerable<AgentStateReadModel> LoadForTeam(Guid teamId);

		IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds);
		IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds);
		IEnumerable<AgentStateReadModel> LoadForSkills(IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> LoadForSitesAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> LoadForTeamsAndSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);

		IEnumerable<AgentStateReadModel> LoadAlarmsForSites(IEnumerable<Guid> siteIds);
		IEnumerable<AgentStateReadModel> LoadAlarmsForTeams(IEnumerable<Guid> teamIds);
		IEnumerable<AgentStateReadModel> LoadAlarmsForSkills(IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> LoadAlarmsForSitesAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> LoadAlarmsForTeamsAndSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);

		IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForSites(IEnumerable<Guid> siteIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForTeams(IEnumerable<Guid> teamIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForSkills(IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForSitesAndSkill(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds);
		IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds);
	}
}