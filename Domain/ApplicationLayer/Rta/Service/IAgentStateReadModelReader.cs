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

		IEnumerable<AgentStateReadModel> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId, string tenant);
		AgentStateReadModel GetCurrentActualAgentState(Guid personId, string tenant);
		IEnumerable<AgentStateReadModel> GetActualAgentStates(string tenant);
	}
}