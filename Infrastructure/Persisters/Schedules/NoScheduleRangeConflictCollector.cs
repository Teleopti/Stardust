using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class NoScheduleRangeConflictCollector : IScheduleRangeConflictCollector
	{
		public IEnumerable<PersistConflict> GetConflicts(IDifferenceCollection<IPersistableScheduleData> differences, IScenario scenario, DateOnlyPeriod period)
		{
			return new List<PersistConflict>();
		}
	}
}