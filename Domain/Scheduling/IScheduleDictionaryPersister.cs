using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IScheduleDictionaryPersister
	{
		IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary);
	}
}