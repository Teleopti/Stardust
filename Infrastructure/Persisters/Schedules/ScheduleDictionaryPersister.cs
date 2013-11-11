using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleDictionaryPersister : IScheduleDictionaryPersister
	{
		private readonly IScheduleRangePersister _scheduleRangePersister;

		public ScheduleDictionaryPersister(IScheduleRangePersister scheduleRangePersister)
		{
			_scheduleRangePersister = scheduleRangePersister;
		}

		public IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary)
		{
			var conflicts = new List<PersistConflict>();
			foreach (var scheduleRange in scheduleDictionary.Values)
			{
				var res = _scheduleRangePersister.Persist(scheduleRange);
				if (res != null)
				{
					conflicts.AddRange(res);					
				}
			}
			return conflicts;
		}
	}
}