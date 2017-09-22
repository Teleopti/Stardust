using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class NoScheduleRangeConflictCollector : IScheduleRangeConflictCollector
	{
		public IEnumerable<PersistConflict> GetConflicts(IDifferenceCollection<IPersistableScheduleData> differences, IScheduleParameters scheduleParameters)
		{
			return Enumerable.Empty<PersistConflict>();
		}
	}
}