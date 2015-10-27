using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeScheduleDifferenceSaver : IScheduleDifferenceSaver
	{
		private readonly IScheduleRepository _scheduleRepository;

		public FakeScheduleDifferenceSaver(IScheduleRepository scheduleRepository)
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

						_scheduleRepository.Remove(scheduleChange.OriginalItem);
						_scheduleRepository.Add(scheduleChange.CurrentItem);

						stateInMemoryUpdater.SolveConflictBecauseOfExternalUpdate(scheduleChange.CurrentItem, true);
						break;
				}
			}
		}
	}
}
