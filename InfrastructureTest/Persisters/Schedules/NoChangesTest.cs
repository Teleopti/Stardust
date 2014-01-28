using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class NoChangesTest : ScheduleRangeConflictTest
	{
		protected override IEnumerable<IAggregateRoot> Given()
		{
			return Enumerable.Empty<IAggregateRoot>();
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			conflicts.Should().Be.Empty();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			myScheduleRange.IsEmpty().Should().Be.True();
		}
	}
}