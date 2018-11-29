using System;
using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class Bug27768 : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2014, 1, 1);

		private IPersonAbsence personAbsence;

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			var start = new DateTime(2014, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2014, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			personAbsence = new PersonAbsence(Person, Scenario, new AbsenceLayer(Absence, new DateTimePeriod(start, end)));
			thisIsTheActualBug_WhenCallingGetHashcode_YouCannotFindTheInstance();
			return new[]{personAbsence};
		}

		private void thisIsTheActualBug_WhenCallingGetHashcode_YouCannotFindTheInstance()
		{
			personAbsence.GetHashCode();
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var scheduleDay = myScheduleRange.ScheduledDay(date);
			scheduleDay.Remove(personAbsence);
			DoModify(scheduleDay);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			conflicts.Should().Be.Empty();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			myScheduleRange.ScheduledDay(date).PersonAbsenceCollection().Should().Be.Empty();

		}
	}
}