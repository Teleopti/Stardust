using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IActivityRepository : IRepository<IActivity >
    {
              IList<IActivity> LoadAllSortByName();
    }
}