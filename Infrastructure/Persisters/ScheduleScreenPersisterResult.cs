using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleScreenPersisterResult : IScheduleScreenPersisterResult
	{
		public IEnumerable<PersistConflict> ScheduleDictionaryConflicts { get; set; }
		public bool Saved { get; set; }
	}
}