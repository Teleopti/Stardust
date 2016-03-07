using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
    public interface IAgentStateReadModelReader
    {
        IList<AgentStateReadModel> Load(IEnumerable<IPerson> persons);
        IList<AgentStateReadModel> Load(IEnumerable<Guid> personIds);
	    IList<AgentStateReadModel> LoadForTeam(Guid teamId);
		IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds, bool? inAlarmOnly, bool? alarmTimeDesc);
		IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds, bool? inAlarmOnly, bool? alarmTimeDesc);

		IEnumerable<AgentStateReadModel> GetAgentsNotInSnapshot(DateTime batchId, string dataSourceId);
		AgentStateReadModel GetCurrentActualAgentState(Guid personId);
		IEnumerable<AgentStateReadModel> GetActualAgentStates();
    }
}