using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
    public interface IActualAgentAssembler : IRtaDataHandlerCache
    {
        IActualAgentState GetAgentState(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode,
                                        DateTime timestamp,
                                        TimeSpan timeInState,
                                        DateTime? batchId, string originalSourceId);

        IEnumerable<IActualAgentState> GetAgentStatesForMissingAgents(DateTime batchId, string sourceId);
        IActualAgentState GetAgentStateForScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp);
    }
}