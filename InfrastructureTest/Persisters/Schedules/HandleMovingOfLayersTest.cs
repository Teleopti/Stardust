using System.Linq;
using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class HandleMovingOfLayersTest : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2001, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			var ass = new PersonAssignment(Person, Scenario, date);
			ass.AddActivity(Activity, new TimePeriod(8, 17));
			ass.AddActivity(Activity, new TimePeriod(9, 18));
			return new[] {ass};
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			var ass = day.PersonAssignment();
			ass.MoveLayerDown(ass.ShiftLayers.First());
			DoModify(day);
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			myScheduleRange.ScheduledDay(date).PersonAssignment().ShiftLayers.First().Period.StartDateTime.Hour
				.Should().Be.EqualTo(9);
		}
	}
}