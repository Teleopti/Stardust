using System;
using System.Collections.Generic;
using NHibernate.Stat;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules.Performance
{
	[Ignore("#47649 Make sure no updates occur when adding new PA layers")]
	public class NoUnnecessaryUpdatesWhenAddingLayer : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2001, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[] { new PersonAssignment(Person, Scenario, date) };
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
		}


		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var start = new DateTime(2001, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var day = myScheduleRange.ScheduledDay(date);
			day.CreateAndAddActivity(Activity, new DateTimePeriod(start, start.AddHours(2)), ShiftCategory);
			DoModify(day);
		}

		protected override void Then(IStatistics statistics)
		{
			//only assignment should be updated
			statistics.EntityUpdateCount.Should().Be.EqualTo(1);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
		}
	}
}