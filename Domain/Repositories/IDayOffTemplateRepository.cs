using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IDayOffTemplateRepository : IRepository<IDayOffTemplate>
    {
        IList<IDayOffTemplate> FindAllDayOffsSortByDescription();
    }
}
