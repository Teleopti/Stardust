using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPreferenceDayRepository : IRepository<IPreferenceDay>, ILoadAggregateById<IPreferenceDay>
    {
        IList<IPreferenceDay> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons);
        IList<IPreferenceDay> Find(DateOnly dateOnly, IPerson person);
    }
}
