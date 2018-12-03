using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class ConflictPublicNoteEagerLoadingTest : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[] { new PublicNote(Person, date, Scenario, "en notering") };
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var day = othersScheduleRange.ScheduledDay(date);
			day.PublicNoteCollection().Single().AppendScheduleNote("mera");
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.PublicNoteCollection().Single().AppendScheduleNote("mer");
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			var dbConflict = (IPublicNote)conflicts.Single().DatabaseVersion;
			LazyLoadingManager.IsInitialized(dbConflict.Person).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.UpdatedBy).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.Scenario).Should().Be.True();
		}
	}
}