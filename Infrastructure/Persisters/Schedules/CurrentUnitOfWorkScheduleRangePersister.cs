using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class CurrentUnitOfWorkScheduleRangePersister : IScheduleRangePersister
	{
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleRangeConflictCollector _scheduleRangeConflictCollector;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IClearScheduleEvents _clearScheduleEvents;

		public CurrentUnitOfWorkScheduleRangePersister(
			IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService,
			IScheduleRangeConflictCollector scheduleRangeConflictCollector,
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			IClearScheduleEvents clearScheduleEvents)
		{
			_differenceCollectionService = differenceCollectionService;
			_scheduleRangeConflictCollector = scheduleRangeConflictCollector;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_clearScheduleEvents = clearScheduleEvents;
		}

		public SchedulePersistResult Persist(IScheduleRange scheduleRange)
		{
			var diff = scheduleRange.DifferenceSinceSnapshot(_differenceCollectionService);
			if (diff.IsEmpty())
			{
				return new SchedulePersistResult();
			}
			var conflicts = _scheduleRangeConflictCollector.GetConflicts(diff, scheduleRange).ToArray();
			if (conflicts.IsEmpty())
			{
				_clearScheduleEvents.Execute(diff);
				_scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate) scheduleRange);
			}
			return new SchedulePersistResult {PersistConflicts = conflicts};
		}
	}
}