using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class NoConflictsOfScheduleRangeConflictCollectorNoneTest : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[] { new PersonAssignment(Person, Scenario, date) };
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var day = othersScheduleRange.ScheduledDay(date);
			day.Clear<IPersonAssignment>();
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.Clear<IPersonAssignment>();
			DoModify(day);
		}

		protected override bool ExpectOptimistLockException => true;

		protected override IScheduleRangeConflictCollector ConflictCollector()
		{
			return new NoScheduleRangeConflictCollector();
		}
	}
}