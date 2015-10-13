using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	//TODO: fake this one or something "lower"?
	public class FakeScheduleDictionaryPersister : IScheduleDictionaryPersister
	{
		public IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary)
		{
			return Enumerable.Empty<PersistConflict>();
		}
	}
}