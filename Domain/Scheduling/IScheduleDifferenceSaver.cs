using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IScheduleDifferenceSaver
	{
		IEnumerable<AggregatedScheduleChangedInfo> SaveChanges(IDifferenceCollection<IPersistableScheduleData> scheduleChanges, IUnvalidatedScheduleRangeUpdate stateInMemoryUpdater);
	}
}