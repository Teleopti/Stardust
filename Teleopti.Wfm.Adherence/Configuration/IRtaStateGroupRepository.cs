using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Configuration
{
    public interface IRtaStateGroupRepository : IRepository<IRtaStateGroup>
    {
        IList<IRtaStateGroup> LoadAllCompleteGraph();
    }
}