using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
    public interface IActualAgentStateCache
    {
        void FlushCacheToDatabase();
        void AddAgentStateToCache(IActualAgentState state);
        bool TryGetLatestState(Guid personId, out IActualAgentState actualAgentState);
    }
}