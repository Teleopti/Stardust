using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public interface IScheduleRangeConflictCollector
	{
		IEnumerable<PersistConflict> GetConflicts(IDifferenceCollection<IPersistableScheduleData> differences, IScheduleParameters scheduleParameters);
	}
}