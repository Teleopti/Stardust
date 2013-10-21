using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class AbsenceConflictTest : ScheduleRangePersisterIntegrationTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override void Given(ICollection<IPersistableScheduleData> scheduleDataInDatabaseAtStart)
		{
			scheduleDataInDatabaseAtStart.Add(new PersonAbsence(Person, Scenario, new AbsenceLayer(Absence, new DateTimePeriod(2000,1,1,2000,1,2))));
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var day = othersScheduleRange.ScheduledDay(date);
			var currAbs = day.PersonAbsenceCollection().Single();
			currAbs.ReplaceLayer(Absence, currAbs.Period.MovePeriod(TimeSpan.FromHours(1)));
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			var currAbs = day.PersonAbsenceCollection().Single();
			currAbs.ReplaceLayer(Absence, currAbs.Period.MovePeriod(TimeSpan.FromHours(2)));
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			var conflict = conflicts.Single();
			conflict.ClientVersion.OriginalItem.Should().Not.Be.Null();
			conflict.ClientVersion.CurrentItem.Should().Not.Be.Null();
			conflict.DatabaseVersion.Should().Not.Be.Null();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			var expectedPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 2).MovePeriod(TimeSpan.FromHours(2));
			myScheduleRange.ScheduledDay(date).PersonAbsenceCollection().Single().Layer.Period
				.Should().Be.EqualTo(expectedPeriod);
		}
	}
}