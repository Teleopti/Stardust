using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IShiftCategoryRepository  : IRepository<IShiftCategory>
    {
        IList<IShiftCategory> FindAll();
    }
}

