using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class NoScheduleRangeConflictCollector : IScheduleRangeConflictCollector
	{
		public NoScheduleRangeConflictCollector()
		{
		}

		public IEnumerable<PersistConflict> GetConflicts(IDifferenceCollection<IPersistableScheduleData> differences, IScheduleParameters scheduleParameters)
		{
			return Enumerable.Empty<PersistConflict>();
		}
	}
}