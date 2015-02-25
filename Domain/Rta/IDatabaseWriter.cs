using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Rta
{
    public interface IDatabaseWriter
    {
        StateCodeInfo AddAndGetStateCode(string stateCode, string stateDescription, Guid platformTypeId, Guid businessUnit);
        void PersistActualAgentReadModel(AgentStateReadModel model);
    }
}