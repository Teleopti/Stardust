using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IOvertimeAvailabilityRepository : IRepository<IOvertimeAvailability>, ILoadAggregateById<IOvertimeAvailability>, ILoadAggregateFromBroker<IOvertimeAvailability>
	{
		IList<IOvertimeAvailability> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons);
		IList<IOvertimeAvailability> Find(DateOnly dateOnly, IPerson person);
	}
}