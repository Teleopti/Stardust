using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IScheduleTagRepository : IRepository<IScheduleTag>
    {
        IList<IScheduleTag> FindAllScheduleTags();
    }
}