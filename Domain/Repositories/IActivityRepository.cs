using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IActivityRepository : IRepository<IActivity >
    {
              IList<IActivity> LoadAllSortByName();
    }
}