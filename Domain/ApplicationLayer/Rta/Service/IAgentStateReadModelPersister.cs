using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
    public interface IAgentStateReadModelPersister
    {
        void PersistActualAgentReadModel(AgentStateReadModel model);
	    void Delete(Guid personId);
    }
}