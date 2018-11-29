using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules.Concurrent
{
	public class DeleteConflictWithUpdateScheduleDataTest : ScheduleRangeConcurrentTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
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
			day.PersonAbsenceCollection().Single().ReplaceLayer(Absence, new DateTimePeriod(1999,12,31,2000,1,2));
			DoModify(day);
		}

		protected override void ThenOneRangeHadConflicts(IScheduleRange unsavedScheduleRangeWithConflicts, IEnumerable<PersistConflict> conflicts,
			bool myScheduleRangeWasTheOneWithConflicts)
		{
			var conflict = conflicts.Single();
			if (myScheduleRangeWasTheOneWithConflicts)
			{
				unsavedScheduleRangeWithConflicts.ScheduledDay(date).PersonAbsenceCollection().Single().Period
								 .Should().Be.EqualTo(new DateTimePeriod(1999, 12, 31, 2000, 1, 2));
				conflict.DatabaseVersion.Should().Be.Null();
				conflict.ClientVersion.CurrentItem.Should().Not.Be.Null();
				conflict.ClientVersion.OriginalItem.Should().Not.Be.Null();
			}
			else
			{
				unsavedScheduleRangeWithConflicts.ScheduledDay(date).PersonAbsenceCollection()
							.Should().Be.Empty();
				conflict.DatabaseVersion.Should().Not.Be.Null();
				conflict.ClientVersion.CurrentItem.Should().Be.Null();
				conflict.ClientVersion.OriginalItem.Should().Not.Be.Null();
			}
		}
	}
}