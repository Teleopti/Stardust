using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PersistSchedulesAfterIsland : ISynchronizeSchedulesAfterIsland
	{
		private readonly IScheduleDictionaryPersister _scheduleDictionaryPersister;
		private readonly ISchedulingSourceScope _schedulingSourceScope;

		public PersistSchedulesAfterIsland(IScheduleDictionaryPersister scheduleDictionaryPersister, ISchedulingSourceScope schedulingSourceScope)
		{
			_scheduleDictionaryPersister = scheduleDictionaryPersister;
			_schedulingSourceScope = schedulingSourceScope;
		}

		public void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period)
		{
			using (_schedulingSourceScope.OnThisThreadUse(ScheduleSource.WebScheduling))
				_scheduleDictionaryPersister.Persist(modifiedScheduleDictionary);
		}
	}
}