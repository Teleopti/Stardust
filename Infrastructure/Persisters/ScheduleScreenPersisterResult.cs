using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleScreenPersisterResult : IScheduleScreenPersisterResult
	{
		public IEnumerable<IPersistConflict> ScheduleDictionaryConflicts { get; set; }
		public bool Saved { get; set; }
	}
}