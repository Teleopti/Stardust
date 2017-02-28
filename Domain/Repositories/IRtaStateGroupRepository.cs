using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IRtaStateGroupRepository : IRepository<IRtaStateGroup>
    {
        IList<IRtaStateGroup> LoadAllCompleteGraph();
    }
}