using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleDictionaryConflictCollector
	{
		IEnumerable<IPersistConflict> GetConflicts(IScheduleDictionary scheduleDictionary, IOwnMessageQueue messageQueueUpdater);
	}
}