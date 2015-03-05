using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Rta
{
    public interface IDatabaseWriter
    {
        void PersistActualAgentReadModel(AgentStateReadModel model);
    }
}