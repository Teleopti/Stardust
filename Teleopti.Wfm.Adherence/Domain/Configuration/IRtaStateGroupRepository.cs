using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Domain.Configuration
{
    public interface IRtaStateGroupRepository : IRepository<IRtaStateGroup>
    {
        IList<IRtaStateGroup> LoadAllCompleteGraph();
    }
}