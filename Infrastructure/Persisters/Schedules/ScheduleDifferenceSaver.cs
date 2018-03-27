using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleDifferenceSaver : IScheduleDifferenceSaver
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IUpdateResourceCalculationReadmodel _updateResourceCalculationReadmodel;

		public ScheduleDifferenceSaver(IScheduleStorage scheduleStorage, ICurrentUnitOfWork currentUnitOfWork,
			IUpdateResourceCalculationReadmodel updateResourceCalculationReadmodel)
		{
			_scheduleStorage = scheduleStorage;
			_currentUnitOfWork = currentUnitOfWork;
			_updateResourceCalculationReadmodel = updateResourceCalculationReadmodel;
		}

		public void SaveChanges(IDifferenceCollection<IPersistableScheduleData> scheduleChanges, IUnvalidatedScheduleRangeUpdate stateInMemoryUpdater)
		{
			_updateResourceCalculationReadmodel.Execute((IScheduleRange)stateInMemoryUpdater);

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
						stateInMemoryUpdater.SolveConflictBecauseOfExternalDeletion(orgItem.Id.GetValueOrDefault(), true);
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