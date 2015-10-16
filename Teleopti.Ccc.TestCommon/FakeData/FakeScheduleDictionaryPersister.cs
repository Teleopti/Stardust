using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	//only work for personassignments ATM
	//if needed, create a new type called SchedulesInDb or something that shares state between "schedule repositories",
	//then we can manipulate that list here
	public class FakeScheduleDictionaryPersister : IScheduleDictionaryPersister
	{
		private readonly IPersonAssignmentRepository _personAssignmentRepository;

		public FakeScheduleDictionaryPersister(IPersonAssignmentRepository personAssignmentRepository)
		{
			_personAssignmentRepository = personAssignmentRepository;
		}

		public IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary)
		{
			var diffSvc = new DifferenceEntityCollectionService<IPersistableScheduleData>();
			foreach (var scheduleRange in scheduleDictionary.Values)
			{
        var diff = scheduleRange.DifferenceSinceSnapshot(diffSvc);
				foreach (var scheduleChange in diff)
				{
					var currAss = (IPersonAssignment)scheduleChange.CurrentItem;
					var orgAss = (IPersonAssignment)scheduleChange.OriginalItem;
					switch (scheduleChange.Status)
					{
						case DifferenceStatus.Added:
							_personAssignmentRepository.Add(currAss);
							break;
						case DifferenceStatus.Deleted:
							_personAssignmentRepository.Remove(orgAss);
							break;
						case DifferenceStatus.Modified:
							_personAssignmentRepository.Remove(orgAss);
							_personAssignmentRepository.Add(currAss);
							break;
					}
				}
			}
			return Enumerable.Empty<PersistConflict>();
		}
	}
}