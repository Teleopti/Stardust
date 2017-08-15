using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	//only work for personassignments ATM
	//if needed, create a new type called SchedulesInDb or something that shares state between "schedule repositories",
	//then we can manipulate that list here
	public class FakeScheduleRangePersister : IScheduleRangePersister
	{
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly object lockToPreventSimultaniousReadWritesToRepoBecauseItShouldNotBeAProblemUsingRealRepository = new object();

		public FakeScheduleRangePersister(IPersonAssignmentRepository personAssignmentRepository)
		{
			_personAssignmentRepository = personAssignmentRepository;
		}

		public SchedulePersistResult Persist(IScheduleRange scheduleRange, DateOnlyPeriod period)
		{
			var diffSvc = new DifferenceEntityCollectionService<IPersistableScheduleData>();

			var diff = scheduleRange.DifferenceSinceSnapshot(diffSvc, period);
			lock (lockToPreventSimultaniousReadWritesToRepoBecauseItShouldNotBeAProblemUsingRealRepository)
			{
				foreach (var scheduleChange in diff)
				{
					var currAss = scheduleChange.CurrentItem as IPersonAssignment;
					if (currAss != null)
					{
						var orgAss = (IPersonAssignment)scheduleChange.OriginalItem;
						switch (scheduleChange.Status)
						{
							case DifferenceStatus.Added:
								_personAssignmentRepository.Add(currAss);
								break;
							case DifferenceStatus.Modified:
								_personAssignmentRepository.Remove(orgAss);
								_personAssignmentRepository.Add(currAss);
								break;
						}
					}
				}
			}
			return new SchedulePersistResult();
		}
	}
}