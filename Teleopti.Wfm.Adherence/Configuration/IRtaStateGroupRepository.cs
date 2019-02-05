using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Configuration
{
    public interface IRtaStateGroupRepository : IRepository<IRtaStateGroup>
    {
        IEnumerable<IRtaStateGroup> LoadAllCompleteGraph();
    }
}