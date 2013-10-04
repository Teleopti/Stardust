using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleDictionarySaver : IScheduleDictionarySaver
	{
		public IScheduleDictionaryPersisterResult MarkForPersist(IUnitOfWork unitOfWork, 
															IScheduleRepository scheduleRepository, 
															IDifferenceCollection<IPersistableScheduleData> scheduleChanges)
		{
			var modifiedEntities = new List<IPersistableScheduleData>();
			var addedEntities = new List<IPersistableScheduleData>();
			var deletedEntities = new List<IPersistableScheduleData>();

			foreach (var diffItem in scheduleChanges)
			{
				if (diffItem.Status == DifferenceStatus.Added)
				{
					// clone entity and add the clone instead, and let the callback handle it with unsafesnapshotupdate.
					// this to not having to handle entity rollback when the transaction is rolled back
					// man, this is ugly!
					var clone = (IPersistableScheduleData) diffItem.CurrentItem.Clone();
					scheduleRepository.Add(clone);
					// ouch.
					// We have to set the id here to make sure unsafesnapshot update works later, otherwise it wont understand its the same entity
					diffItem.CurrentItem.SetId(clone.Id);
					addedEntities.Add(clone);
					continue;

					//todo- change to this if it works
					// scheduleRepository.Add(diffItem.CurrentItem);
					// addedEntities.Add(diffItem.CurrentItem);
				}
				if (diffItem.Status == DifferenceStatus.Deleted)
				{
					scheduleRepository.Remove(diffItem.OriginalItem);
					deletedEntities.Add(diffItem.OriginalItem);
					continue;
				}
				if (diffItem.Status == DifferenceStatus.Modified)
				{
					unitOfWork.Reassociate(diffItem.OriginalItem);
					var merged = unitOfWork.Merge(diffItem.CurrentItem);
					modifiedEntities.Add(merged);

					continue;
				}
			}

			return new ScheduleDictionaryPersisterResult { ModifiedEntities = modifiedEntities, AddedEntities = addedEntities, DeletedEntities = deletedEntities};
		}
	}
}