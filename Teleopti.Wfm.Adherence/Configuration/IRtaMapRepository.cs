using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Configuration
{
    public interface IRtaMapRepository : IRepository<IRtaMap  >
    {
        IList<IRtaMap> LoadAllCompleteGraph();
    }
}