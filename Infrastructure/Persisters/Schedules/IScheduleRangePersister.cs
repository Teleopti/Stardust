using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public interface IScheduleRangePersister
	{
		IEnumerable<PersistConflict> Persist(IScheduleRange scheduleRange);
	}
}