using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleScreenPersisterResult 
	{
		IEnumerable<IPersistConflict> ScheduleDictionaryConflicts { get; set; }
		bool Saved { get; set; }
	}
}