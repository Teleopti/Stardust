using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class CurrentUnitOfWorkScheduleRangePersister : IScheduleRangePersister
	{
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleRangeConflictCollector _scheduleRangeConflictCollector;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public CurrentUnitOfWorkScheduleRangePersister(IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService,
			IScheduleRangeConflictCollector scheduleRangeConflictCollector,
			IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_differenceCollectionService = differenceCollectionService;
			_scheduleRangeConflictCollector = scheduleRangeConflictCollector;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public IEnumerable<PersistConflict> Persist(IScheduleRange scheduleRange)
		{
			var diff = scheduleRange.DifferenceSinceSnapshot(_differenceCollectionService);
			if (diff.IsNullOrEmpty())
			{
				return Enumerable.Empty<PersistConflict>();
			}
			var conflicts = _scheduleRangeConflictCollector.GetConflicts(diff, scheduleRange).ToArray();
			if (conflicts.IsNullOrEmpty())
			{
				_scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate) scheduleRange);
			}
			return conflicts;
		}
	}
}