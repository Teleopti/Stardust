using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
    public interface IDatabaseWriter
    {
        RtaStateGroupLight AddAndGetNewRtaState(string stateCode, Guid platformTypeId, Guid businessUnit);
        void AddOrUpdate(IEnumerable<IActualAgentState> actualAgentStates);
    }
}