using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleRangeSaver : IScheduleRangeSaver
	{
		private readonly IScheduleRepository _scheduleRepository;

		public ScheduleRangeSaver(IScheduleRepository scheduleRepository)
		{
			_scheduleRepository = scheduleRepository;
		}

		public void SaveChanges(IDifferenceCollection<IPersistableScheduleData> scheduleChanges, IUnvalidatedScheduleRangeUpdate stateInMemoryUpdater)
		{
			foreach (var scheduleChange in scheduleChanges)
			{
				switch (scheduleChange.Status)
				{
					case DifferenceStatus.Added:
						var currentItem = scheduleChange.CurrentItem;
						_scheduleRepository.Add(currentItem);
						stateInMemoryUpdater.SolveConflictBecauseOfExternalInsert(currentItem, true);
						break;
					case DifferenceStatus.Deleted:
						var orgItem = scheduleChange.OriginalItem;
						_scheduleRepository.Remove(orgItem);
						stateInMemoryUpdater.SolveConflictBecauseOfExternalDeletion(orgItem.Id.Value, true);
						break;
					case DifferenceStatus.Modified:
						var unitOfWork = _scheduleRepository.UnitOfWork;
						unitOfWork.Reassociate(scheduleChange.OriginalItem);
						var merged = unitOfWork.Merge(scheduleChange.CurrentItem);
						stateInMemoryUpdater.SolveConflictBecauseOfExternalUpdate(merged, true);
						break;
				}
			}
		}
	}
}