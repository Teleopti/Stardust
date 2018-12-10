using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules.Concurrent
{
	public class DeleteConflictWithUpdateAssignmentTest : ScheduleRangeConcurrentTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[] { new PersonAssignment(Person, Scenario, date) };
		}

		protected override void WhenOtherIsChanging(IScheduleRange othersScheduleRange)
		{
			var start = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var day = othersScheduleRange.ScheduledDay(date);
			day.PersonAssignment().AddActivity(Activity, new DateTimePeriod(start, start.AddHours(3)));
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.Clear<IPersonAssignment>();
			DoModify(day);
		}

		protected override void ThenOneRangeHadConflicts(IScheduleRange unsavedScheduleRangeWithConflicts, 
																										IEnumerable<PersistConflict> conflicts,
																										bool myScheduleRangeWasTheOneWithConflicts)
		{
			var conflict = conflicts.Single();
			if (myScheduleRangeWasTheOneWithConflicts)
			{
				unsavedScheduleRangeWithConflicts.ScheduledDay(date).PersonAssignment()
								 .Should().Be.Null();
				conflict.DatabaseVersion.Should().Not.Be.Null();
				conflict.ClientVersion.CurrentItem.Should().Be.Null();
				conflict.ClientVersion.OriginalItem.Should().Not.Be.Null();
			}
			else
			{
				unsavedScheduleRangeWithConflicts.ScheduledDay(date).PersonAssignment().MainActivities().Single().Payload
							.Should().Be.EqualTo(Activity);
				conflict.DatabaseVersion.Should().Be.Null();
				conflict.ClientVersion.CurrentItem.Should().Not.Be.Null();
				conflict.ClientVersion.OriginalItem.Should().Not.Be.Null();
			}
		}
	}
}