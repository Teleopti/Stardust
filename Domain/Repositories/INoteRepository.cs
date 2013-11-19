using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface INoteRepository : IRepository<INote>, ILoadAggregateById<INote>, ILoadAggregateFromBroker<INote>
    {
        IList<INote> Find(DateTimePeriod period, IScenario scenario);
        ICollection<INote> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario);
    }
}
