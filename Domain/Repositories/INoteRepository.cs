using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface INoteRepository : IRepository<INote>, ILoadAggregateById<INote>, ILoadAggregateFromBroker<INote>
    {
        ICollection<INote> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario);
    }
}
