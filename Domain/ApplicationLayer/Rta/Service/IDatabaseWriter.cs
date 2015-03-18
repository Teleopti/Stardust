using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
    public interface IDatabaseWriter
    {
		 void PersistActualAgentReadModel(AgentStateReadModel model, string tenant);
    }
}