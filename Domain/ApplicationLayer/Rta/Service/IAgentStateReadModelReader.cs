using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
    public interface IAgentStateReadModelReader
    {
        IList<AgentStateReadModel> Load(IEnumerable<IPerson> persons);
        IList<AgentStateReadModel> Load(IEnumerable<Guid> personGuids);
	    IList<AgentStateReadModel> LoadForTeam(Guid teamId);
		IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds, bool? inAlarmOnly = null);
		IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds, bool? inAlarmOnly = null);

		IEnumerable<AgentStateReadModel> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId);

		AgentStateReadModel GetCurrentActualAgentState(Guid personId);
		IEnumerable<AgentStateReadModel> GetActualAgentStates();
    }
}