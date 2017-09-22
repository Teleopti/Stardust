using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public interface IScheduleRangeConflictCollector
	{
		IEnumerable<PersistConflict> GetConflicts(IDifferenceCollection<IPersistableScheduleData> differences, IScheduleParameters scheduleParameters);
	}
}