using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeScheduleDifferenceSaver : IScheduleDifferenceSaver
	{
		private readonly IScheduleStorage _scheduleStorage;

		public FakeScheduleDifferenceSaver(IScheduleStorage scheduleStorage)
		{
			_scheduleStorage = scheduleStorage;
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

						_scheduleStorage.Remove(scheduleChange.OriginalItem);
						_scheduleStorage.Add(scheduleChange.CurrentItem);

						stateInMemoryUpdater.SolveConflictBecauseOfExternalUpdate(scheduleChange.CurrentItem, true);
						break;
				}
			}
		}
	}
}
