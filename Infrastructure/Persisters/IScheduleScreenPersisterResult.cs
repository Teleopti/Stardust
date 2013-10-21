using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleScreenPersisterResult 
	{
		IEnumerable<PersistConflict> ScheduleDictionaryConflicts { get; set; }
		bool Saved { get; set; }
	}
}