using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Rta
{
    public interface IRtaRepository
    {
        IList<IActualAgentState> LoadActualAgentState(IEnumerable<IPerson> persons);
        IList<IActualAgentState> LoadLastAgentState(IEnumerable<Guid> personGuids);
    }
}