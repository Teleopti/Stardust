using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleDictionaryPersister : IScheduleDictionaryPersister
	{
		private readonly IScheduleRangePersister _scheduleRangePersister;
		private readonly IAggregatedScheduleChangeSender _aggregatedScheduleChangeSender;

		public ScheduleDictionaryPersister(
			IScheduleRangePersister scheduleRangePersister,
			IAggregatedScheduleChangeSender aggregatedScheduleChangeSender)
		{
			_scheduleRangePersister = scheduleRangePersister;
			_aggregatedScheduleChangeSender = aggregatedScheduleChangeSender;
		}

		public IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary)
		{
			var conflicts = new List<PersistConflict>();
			var modified = new List<AggregatedScheduleChangedInfo>();
			IScenario scenario = null;
			foreach (var scheduleRange in scheduleDictionary.Values)
			{
				scenario = scheduleRange.Scenario;
				var res = _scheduleRangePersister.Persist(scheduleRange, modified);
				if (res != null)
				{
					conflicts.AddRange(res);					
				}
			}
			_aggregatedScheduleChangeSender.Send(modified, scenario);
			return conflicts;
		}
	}
}