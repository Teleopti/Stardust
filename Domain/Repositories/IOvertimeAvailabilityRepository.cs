using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IOvertimeAvailabilityRepository : IRepository<IOvertimeAvailability>, ILoadAggregateById<IOvertimeAvailability>, ILoadAggregateFromBroker<IOvertimeAvailability>
	{
		IList<IOvertimeAvailability> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons);
		IList<IOvertimeAvailability> Find(DateOnly dateOnly, IPerson person);
		IList<IOvertimeAvailability> Find(DateOnlyPeriod period);
	}
}