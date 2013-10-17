using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public interface IScheduleRangeConflictCollector
	{
		IEnumerable<PersistConflict> GetConflicts(IScheduleRange scheduleRange);
	}

	public class ScheduleRangeConflictCollector : IScheduleRangeConflictCollector
	{
		public IEnumerable<PersistConflict> GetConflicts(IScheduleRange scheduleRange)
		{
			return Enumerable.Empty<PersistConflict>();
		}
	}
}