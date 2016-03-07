using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
    public interface IAgentStateReadModelPersister
    {
        void PersistActualAgentReadModel(AgentStateReadModel model);
		IEnumerable<AgentStateReadModel> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId);
		AgentStateReadModel GetCurrentActualAgentState(Guid personId);
		IEnumerable<AgentStateReadModel> GetActualAgentStates();
		void Delete(Guid personId);
    }
}