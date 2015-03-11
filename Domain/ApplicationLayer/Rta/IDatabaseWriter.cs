using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
    public interface IDatabaseWriter
    {
        void PersistActualAgentReadModel(AgentStateReadModel model);
    }
}