using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class ConflictPersonAbsenceEagerLoadingTest : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[] { new PersonAbsence(Person, Scenario, new AbsenceLayer(Absence, new DateTimePeriod(2000, 1, 1, 2000, 1, 2))) };
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
			var dbConflict = (IPersonAbsence)conflicts.Single().DatabaseVersion;
			LazyLoadingManager.IsInitialized(dbConflict.Person).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.UpdatedBy).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.Layer.Payload).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.Scenario).Should().Be.True();
		}
	}
}