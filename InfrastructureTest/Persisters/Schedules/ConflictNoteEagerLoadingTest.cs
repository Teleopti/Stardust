using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class ConflictNoteEagerLoadingTest :ScheduleRangePersisterBaseTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override void Given(ICollection<IPersistableScheduleData> scheduleDataInDatabaseAtStart)
		{
			scheduleDataInDatabaseAtStart.Add(new Note(Person, date, Scenario,"en notering"));
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var day = othersScheduleRange.ScheduledDay(date);
			day.NoteCollection().Single().AppendScheduleNote("mera");
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.NoteCollection().Single().AppendScheduleNote("mer");
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			var dbConflict = (INote)conflicts.Single().DatabaseVersion;
			LazyLoadingManager.IsInitialized(dbConflict.Person).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.UpdatedBy).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.Scenario).Should().Be.True();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
		}
	}
}