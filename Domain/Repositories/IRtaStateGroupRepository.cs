using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IRtaStateGroupRepository : IRepository<IRtaStateGroup>
    {
        IList<IRtaStateGroup> LoadAllCompleteGraph();
        IEnumerable<IRtaStateGroup> LoadAllExclusive();
    }
}