using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IScheduleTagRepository : IRepository<IScheduleTag>
    {
        IList<IScheduleTag> FindAllScheduleTags();
    }
}