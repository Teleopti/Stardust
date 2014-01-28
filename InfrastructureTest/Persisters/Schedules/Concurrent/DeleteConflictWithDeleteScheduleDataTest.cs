using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules.Concurrent
{
	public class DeleteConflictWithDeleteScheduleDataTest : ScheduleRangeConcurrentTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IAggregateRoot> Given()
		{
			return new[] { new PersonAbsence(Person, Scenario, new AbsenceLayer(Absence, new DateTimePeriod(2000, 1, 1, 2000, 1, 2))) };
		}

		protected override void WhenOtherIsChanging(IScheduleRange othersScheduleRange)
		{
			var day = othersScheduleRange.ScheduledDay(date);
			day.Clear<IPersonAbsence>();
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.Clear<IPersonAbsence>();
			DoModify(day);
		}

		protected override void ThenOneRangeHadConflicts(IScheduleRange unsavedScheduleRangeWithConflicts, IEnumerable<PersistConflict> conflicts,
			bool myScheduleRangeWasTheOneWithConflicts)
		{
			unsavedScheduleRangeWithConflicts.ScheduledDay(date).PersonAbsenceCollection().Should().Be.Empty();

			var conflict = conflicts.Single();
			conflict.DatabaseVersion.Should().Be.Null();
			conflict.ClientVersion.CurrentItem.Should().Be.Null();
			conflict.ClientVersion.OriginalItem.Should().Not.Be.Null();
		}
	}
}