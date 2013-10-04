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
					// clone entity and add the clone instead, and let the callback handle it with unsafesnapshotupdate.
					// this to not having to handle entity rollback when the transaction is rolled back
					// man, this is ugly!
					var clone = (IPersistableScheduleData)scheduleChange.CurrentItem.Clone();
					scheduleRepository.Add(clone);
					// ouch.
					// We have to set the id here to make sure unsafesnapshot update works later, otherwise it wont understand its the same entity
					scheduleChange.CurrentItem.SetId(clone.Id);
					addedEntities.Add(clone);

					//todo- change to this if it works
					// scheduleRepository.Add(diffItem.CurrentItem);
					// addedEntities.Add(diffItem.CurrentItem);

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