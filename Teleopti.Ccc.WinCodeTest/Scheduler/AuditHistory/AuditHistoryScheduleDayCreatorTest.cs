using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AuditHistory;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.AuditHistory
{

	[TestFixture]
	public class AuditHistoryScheduleDayCreatorTest
	{
		private IScheduleDay scheduleDay;
		private IAuditHistoryScheduleDayCreator target;

		[SetUp]
		public void Setup()
		{
			scheduleDay = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			target = new AuditHistoryScheduleDayCreator();
		}

		[Test]
		public void ShouldReplaceAbsences()
		{
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod()));
			var newAbsences = new[]
				{
					new PersonAbsence(scheduleDay.Person, scheduleDay.Scenario, new AbsenceLayer(new Absence(), new DateTimePeriod(2000,1,1,2000,1,2))),
					new PersonAbsence(scheduleDay.Person, scheduleDay.Scenario, new AbsenceLayer(new Absence(), new DateTimePeriod(2000,1,1,2000,1,2)))
				};
			target.Apply(scheduleDay, newAbsences);
			scheduleDay.PersonAbsenceCollection().Length.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReplaceAssignment()
		{
			scheduleDay.CreateAndAddActivity(new Activity("no"), new DateTimePeriod(2000,1,1,2000,1,2), new ShiftCategory("sdf"));
			var newAss = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			newAss.AddActivity(new Activity("yes"), new DateTimePeriod(2000,1,1,2000,1,2));
			newAss.SetShiftCategory(new ShiftCategory("sdf"));
			target.Apply(scheduleDay, new[]{newAss});
			scheduleDay.PersonAssignment().MainActivities().Single().Payload.Name.Should().Be.EqualTo("yes");
		}

		[Test]
		public void ShouldKeepOriginalPersonAssignmentId()
		{
			var ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.SetId(Guid.NewGuid());
			scheduleDay.Add(ass);
			target.Apply(scheduleDay, new[]{new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1))});
			scheduleDay.PersonAssignment().Id.Should().Be.EqualTo(ass.Id);
		}

		[Test]
		public void ShouldKeepOriginalPersonAssignmentVersion()
		{
			var ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.SetVersion(123);
			scheduleDay.Add(ass);
			target.Apply(scheduleDay, new[] { new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1)) });
			scheduleDay.PersonAssignment().Version.Should().Be.EqualTo(ass.Version);
		}


		[Test]
		public void ShouldKeepNote()
		{
			scheduleDay.CreateAndAddNote("hej");
			target.Apply(scheduleDay, Enumerable.Empty<IPersistableScheduleData>());
			scheduleDay.NoteCollection().Should().Not.Be.Empty();
		}
	}
}