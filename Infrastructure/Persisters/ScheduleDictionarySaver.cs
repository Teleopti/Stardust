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
					//BUG 25007 - see Teleopti.Ccc.InfrastructureTest.Persisters.BugTest.Bug25007
					//we (Roger & Mathias) don't know why though... Now we have a test a least

					var clone = (IPersistableScheduleData)diffItem.CurrentItem.Clone();
					scheduleRepository.Add(clone);
					diffItem.CurrentItem.SetId(clone.Id);
					addedEntities.Add(clone);
					continue;
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