using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IStudentAvailabilityDayRepository : IRepository<IStudentAvailabilityDay>, ILoadAggregateById<IStudentAvailabilityDay>, ILoadAggregateFromBroker<IStudentAvailabilityDay>
    {
        IList<IStudentAvailabilityDay> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons);
        IList<IStudentAvailabilityDay> Find(DateOnly dateOnly, IPerson person);
	    IList<IStudentAvailabilityDay> FindNewerThan(DateTime newerThan);
    }
}
