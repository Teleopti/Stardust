using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Rta
{
    public interface IRtaRepository
    {
        IList<AgentStateReadModel> LoadActualAgentState(IEnumerable<IPerson> persons);
        IList<AgentStateReadModel> LoadLastAgentState(IEnumerable<Guid> personGuids);
    }
}