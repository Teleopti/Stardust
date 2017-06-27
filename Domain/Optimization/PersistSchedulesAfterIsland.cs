using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PersistSchedulesAfterIsland : ISynchronizeSchedulesAfterIsland
	{
		private readonly IScheduleDictionaryPersister _scheduleDictionaryPersister;

		public PersistSchedulesAfterIsland(IScheduleDictionaryPersister scheduleDictionaryPersister)
		{
			_scheduleDictionaryPersister = scheduleDictionaryPersister;
		}

		public void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period)
		{
			_scheduleDictionaryPersister.Persist(modifiedScheduleDictionary);
		}
	}
}