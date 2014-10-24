using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IRtaRepository
    {
        IList<IActualAgentState> LoadActualAgentState(IEnumerable<IPerson> persons);
        IList<IActualAgentState> LoadLastAgentState(IEnumerable<Guid> personGuids);
        IActualAgentState LoadOneActualAgentState(Guid value);
        void AddOrUpdateActualAgentState(IActualAgentState actualAgentState);
    }
}