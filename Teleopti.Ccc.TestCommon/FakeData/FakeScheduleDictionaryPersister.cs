using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	//this thing might need some more hugs and knådning
	public class FakeScheduleDictionaryPersister : IScheduleDictionaryPersister
	{
		private readonly IScheduleRepository _scheduleRepository;

		public FakeScheduleDictionaryPersister(IScheduleRepository scheduleRepository)
		{
			_scheduleRepository = scheduleRepository;
		}

		public IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary)
		{
			var diffSvc = new DifferenceEntityCollectionService<IPersistableScheduleData>();
			foreach (var scheduleRange in scheduleDictionary.Values)
			{
        var diff = scheduleRange.DifferenceSinceSnapshot(diffSvc);
				foreach (var scheduleChange in diff)
				{
					switch (scheduleChange.Status)
					{
						case DifferenceStatus.Added:
							_scheduleRepository.Add(scheduleChange.CurrentItem);
							break;
						case DifferenceStatus.Deleted:
							_scheduleRepository.Remove(scheduleChange.OriginalItem);
							break;
						case DifferenceStatus.Modified:
							break;
					}
				}
			}
			return Enumerable.Empty<PersistConflict>();
		}
	}
}