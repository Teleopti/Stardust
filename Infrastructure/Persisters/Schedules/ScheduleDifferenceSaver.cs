using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleDifferenceSaver : IScheduleDifferenceSaver
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IScheduleDayDifferenceSaver _scheduleDayDifferenceSaver;

		public ScheduleDifferenceSaver(IScheduleStorage scheduleStorage, ICurrentUnitOfWork currentUnitOfWork, IScheduleDayDifferenceSaver scheduleDayDifferenceSaver)
		{
			_scheduleStorage = scheduleStorage;
			_currentUnitOfWork = currentUnitOfWork;
			_scheduleDayDifferenceSaver = scheduleDayDifferenceSaver;
		}

		public void SaveChanges(IDifferenceCollection<IPersistableScheduleData> scheduleChanges, IUnvalidatedScheduleRangeUpdate stateInMemoryUpdater)
		{
			var scheduleRange = stateInMemoryUpdater as IScheduleRange;
			_scheduleDayDifferenceSaver.SaveDifferences(scheduleRange);
			foreach (var scheduleChange in scheduleChanges)
			{
				switch (scheduleChange.Status)
				{
					case DifferenceStatus.Added:
						var currentItem = scheduleChange.CurrentItem;
						_scheduleStorage.Add(currentItem);
						stateInMemoryUpdater.SolveConflictBecauseOfExternalInsert(currentItem, true);
						break;
					case DifferenceStatus.Deleted:
						var orgItem = scheduleChange.OriginalItem;
						_scheduleStorage.Remove(orgItem);
						stateInMemoryUpdater.SolveConflictBecauseOfExternalDeletion(orgItem.Id.Value, true);
						break;
					case DifferenceStatus.Modified:
						var unitOfWork = _currentUnitOfWork.Current();
						unitOfWork.Reassociate(scheduleChange.OriginalItem);
						var merged = unitOfWork.Merge(scheduleChange.CurrentItem);
						stateInMemoryUpdater.SolveConflictBecauseOfExternalUpdate(merged, true);
						break;
				}
			}
		}
	}
	
}