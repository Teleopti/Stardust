using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleDictionaryConflictCollector
	{
		IEnumerable<PersistConflict> GetConflicts(IScheduleDictionary scheduleDictionary, IOwnMessageQueue messageQueueUpdater);
	}
}