using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
    public interface IAgentStateReadModelPersister
    {
        void Persist(AgentStateReadModel model);
		AgentStateReadModel Get(Guid personId);
		IEnumerable<AgentStateReadModel> GetAll();
		IEnumerable<AgentStateReadModel> GetNotInSnapshot(DateTime batchId, string dataSourceId);
		void Delete(Guid personId);
	}
}