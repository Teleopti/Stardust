using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class DeleteNoConflictTest : ScheduleRangePersisterIntegrationTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new []{new PersonAssignment(Person, Scenario, date)};
		}

		protected override IEnumerable<IScheduleDay> WhenI(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(new DateOnly(2000, 1, 1));
			day.Clear<IPersonAssignment>();
			return new[]{day};
		}

		protected override IEnumerable<IScheduleDay> WhenOther(IScheduleRange othersScheduleRange)
		{
			return Enumerable.Empty<IScheduleDay>();
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts, IScheduleRange scheduleRangeInMemory, IScheduleRange scheduleRangeInDatabase)
		{

		}
	}
}