using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeScheduleDifferenceSaver : IScheduleDifferenceSaver
	{
		private IScheduleStorage _scheduleStorage;
		private readonly IScheduleDayDifferenceSaver _scheduleDayDifferenceSaver;

		public FakeScheduleDifferenceSaver(IScheduleStorage scheduleStorage, IScheduleDayDifferenceSaver scheduleDayDifferenceSaver)
		{
			_scheduleStorage = scheduleStorage;
			_scheduleDayDifferenceSaver = scheduleDayDifferenceSaver;
		}

		public FakeScheduleDifferenceSaver()
		{			
		}

		public void SetScheduleStorage(IScheduleStorage scheduleStorage)
		{
			_scheduleStorage = scheduleStorage;
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

						_scheduleStorage.Remove(scheduleChange.OriginalItem);
						_scheduleStorage.Add(scheduleChange.CurrentItem);

						stateInMemoryUpdater.SolveConflictBecauseOfExternalUpdate(scheduleChange.CurrentItem, true);
						break;
				}
			}
		}
	}
}
