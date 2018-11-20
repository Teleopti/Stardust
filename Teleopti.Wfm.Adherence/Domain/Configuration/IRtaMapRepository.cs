using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Domain.Configuration
{
    public interface IRtaMapRepository : IRepository<IRtaMap  >
    {
        IList<IRtaMap> LoadAllCompleteGraph();
    }
}