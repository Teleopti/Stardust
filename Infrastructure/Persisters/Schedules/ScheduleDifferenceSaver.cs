using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleDifferenceSaver : IScheduleDifferenceSaver
	{
		private readonly IScheduleRepository _scheduleRepository;

		public ScheduleDifferenceSaver(IScheduleRepository scheduleRepository)
		{
			_scheduleRepository = scheduleRepository;
		}

		public IEnumerable<AggregatedScheduleChangedInfo> SaveChanges(IDifferenceCollection<IPersistableScheduleData> scheduleChanges, IUnvalidatedScheduleRangeUpdate stateInMemoryUpdater)
		{
			var modified = new List<AggregatedScheduleChangedInfo>();
			foreach (var scheduleChange in scheduleChanges)
			{
				switch (scheduleChange.Status)
				{
					case DifferenceStatus.Added:
						var currentItem = scheduleChange.CurrentItem;
						_scheduleRepository.Add(currentItem);
						modified.Add(new AggregatedScheduleChangedInfo { PersonId = currentItem.Person.Id.Value, Period = currentItem.Period });
						stateInMemoryUpdater.SolveConflictBecauseOfExternalInsert(currentItem, true);
						break;
					case DifferenceStatus.Deleted:
						var orgItem = scheduleChange.OriginalItem;
						_scheduleRepository.Remove(orgItem);
						modified.Add(new AggregatedScheduleChangedInfo { PersonId = orgItem.Person.Id.Value, Period = orgItem.Period });
						stateInMemoryUpdater.SolveConflictBecauseOfExternalDeletion(orgItem.Id.Value, true);
						break;
					case DifferenceStatus.Modified:
						var unitOfWork = _scheduleRepository.UnitOfWork;
						unitOfWork.Reassociate(scheduleChange.OriginalItem);
						var merged = unitOfWork.Merge(scheduleChange.CurrentItem);
						modified.Add(new AggregatedScheduleChangedInfo { PersonId = merged.Person.Id.Value, Period = merged.Period });
						stateInMemoryUpdater.SolveConflictBecauseOfExternalUpdate(merged, true);
						break;
				}
			}
			return modified;
		}
	}
}