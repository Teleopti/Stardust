using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class AddScheduleDataNoConflictTest : ScheduleRangePersisterIntegrationTest
	{
		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return Enumerable.Empty<IPersistableScheduleData>();
		}

		protected override IEnumerable<IPersistableScheduleData> When()
		{
			return new[]
				{
					new PersonAssignment(Person, Scenario, new DateOnly(2000, 1, 1))
				};
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts, IScheduleRange scheduleRangeInMemory, IScheduleRange scheduleRangeInDatabase)
		{
			conflicts.Should().Be.Empty();
			scheduleRangeInMemory.ScheduledDay(new DateOnly(2000, 1, 1)).PersonAssignment().Should().Not.Be.Null();
			scheduleRangeInDatabase.ScheduledDay(new DateOnly(2000, 1, 1)).PersonAssignment().Should().Not.Be.Null();
		}
	}
}