using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleRangePersister : IScheduleRangePersister
	{
		public IEnumerable<PersistConflict> Persist(IScheduleRange scheduleRange, List<AggregatedScheduleChangedInfo> modifiedPersonAssignments)
		{
			return new List<PersistConflict>();
		}
	}
}