using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class DeleteOneOfManyLayersTest: ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[]
			{
				new PersonAssignment(Person, Scenario, date)
					.WithLayer(Activity, new TimePeriod(1, 10))
					.WithLayer(Activity, new TimePeriod(2, 10))
					.WithLayer(Activity, new TimePeriod(3, 10))
			};
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(new DateOnly(2000, 1, 1));
			var layer2remove = day.PersonAssignment().ShiftLayers.First();
			day.PersonAssignment().RemoveActivity(layer2remove);
			DoModify(day);
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			myScheduleRange.ScheduledDay(date).PersonAssignment().ShiftLayers
				.Select(x => x.Period.StartDateTime.Hour)
				.Should().Have.SameSequenceAs(2, 3);
		}
	}
}