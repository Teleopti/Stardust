using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public interface IScheduleDictionaryPersister
	{
		IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary);
	}
}