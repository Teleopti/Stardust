using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class NoChangesTest : ScheduleRangePersisterIntegrationTest
	{
		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return Enumerable.Empty<IPersistableScheduleData>();
		}

		protected override IEnumerable<IScheduleDay> When(IScheduleRange scheduleRange)
		{
			return Enumerable.Empty<IScheduleDay>();
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts, IScheduleRange scheduleRangeInMemory, IScheduleRange scheduleRangeInDatabase)
		{
			conflicts.Should().Be.Empty();
			scheduleRangeInMemory.IsEmpty().Should().Be.True();
			scheduleRangeInDatabase.IsEmpty().Should().Be.True();
		}
	}
}