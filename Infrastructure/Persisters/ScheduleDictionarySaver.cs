using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleDictionarySaver : IScheduleDictionarySaver
	{
		public IScheduleDictionaryPersisterResult MarkForPersist(IUnitOfWork unitOfWork,
															IScheduleRepository scheduleRepository,
															DifferenceCollectionItem<IPersistableScheduleData> scheduleChange)
		{
			var modifiedEntities = new List<IPersistableScheduleData>();
			var addedEntities = new List<IPersistableScheduleData>();
			var deletedEntities = new List<IPersistableScheduleData>();

			switch (scheduleChange.Status)
			{
				case DifferenceStatus.Added:
					//BUG 25007 - see Teleopti.Ccc.InfrastructureTest.Persisters.BugTest.Bug25007
					//we (Roger & Mathias) don't know why though... Now we have a test a least
					var clone = (IPersistableScheduleData)scheduleChange.CurrentItem.Clone();
					scheduleRepository.Add(clone);
					scheduleChange.CurrentItem.SetId(clone.Id);
					addedEntities.Add(clone);
					break;
				case DifferenceStatus.Deleted:
					scheduleRepository.Remove(scheduleChange.OriginalItem);
					deletedEntities.Add(scheduleChange.OriginalItem);
					break;
				case DifferenceStatus.Modified:
					unitOfWork.Reassociate(scheduleChange.OriginalItem);
					var merged = unitOfWork.Merge(scheduleChange.CurrentItem);
					modifiedEntities.Add(merged);
					break;
			}

			return new ScheduleDictionaryPersisterResult { ModifiedEntities = modifiedEntities, AddedEntities = addedEntities, DeletedEntities = deletedEntities };
		}
	}
}