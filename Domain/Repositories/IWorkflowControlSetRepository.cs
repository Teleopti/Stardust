using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IWorkflowControlSetRepository : IRepository<IWorkflowControlSet>
    {
        IList<IWorkflowControlSet> LoadAllSortByName();
    }
}