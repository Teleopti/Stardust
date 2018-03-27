using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.TestCommon
{
	//don't use. to start using real impl, you need to set Id on your assignments in the repository 
	//(which makes sense anyway)
	public class FakeScheduleDifferenceSaver_DoNotUse : IScheduleDifferenceSaver
	{
		private IScheduleStorage _scheduleStorage;

		public FakeScheduleDifferenceSaver_DoNotUse(IScheduleStorage scheduleStorage)
		{
			_scheduleStorage = scheduleStorage;
		}


		public void SetScheduleStorage(IScheduleStorage scheduleStorage)
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
