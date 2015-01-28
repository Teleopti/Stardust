using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Rta
{
    public interface IDatabaseWriter
    {
        RtaStateGroupLight AddAndGetNewRtaState(string stateCode, Guid platformTypeId, Guid businessUnit);
        void PersistActualAgentReadModel(AgentStateReadModel model);
    }
}