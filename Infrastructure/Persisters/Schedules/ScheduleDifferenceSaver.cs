using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleDifferenceSaver : IScheduleDifferenceSaver
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ScheduleDifferenceSaver(IScheduleStorage scheduleStorage, ICurrentUnitOfWork currentUnitOfWork)
		{
			_scheduleStorage = scheduleStorage;
			_currentUnitOfWork = currentUnitOfWork;
		}

		public void SaveChanges(IDifferenceCollection<IPersistableScheduleData> scheduleChanges, IUnvalidatedScheduleRangeUpdate stateInMemoryUpdater)
		{
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