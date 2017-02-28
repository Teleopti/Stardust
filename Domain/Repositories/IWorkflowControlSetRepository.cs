using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IWorkflowControlSetRepository : IRepository<IWorkflowControlSet>
    {
        IList<IWorkflowControlSet> LoadAllSortByName();
    }
}