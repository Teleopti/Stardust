using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	[TestFixture]
	public class PersonAssignmentDateSetterTest : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldSetCorrectDateForResettedPersonAssignment()
		{
			var paRep = new PersonAssignmentRepository(UnitOfWork);
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expected = new DateOnly(2000, 1, 1);

			var pa = createAndStoreAssignment(start);

			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(PersonAssignmentDateSetter.DateOfUnconvertedSchedule);
			UnitOfWork.Clear();

			new PersonAssignmentDateSetter().ExecutePersonAssignmentSetterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.Utc);

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldNotTouchAssignmentIfNotRestoreDate()
		{
			var paRep = new PersonAssignmentRepository(UnitOfWork);
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expected = new DateOnly(2000, 1, 1);

			var pa = createAndStoreAssignment(start);

			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			new PersonAssignmentDateSetter().ExecutePersonAssignmentSetterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.Utc);

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldNotConvertWrongPerson()
		{
			var paRep = new PersonAssignmentRepository(UnitOfWork);
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);

			var pa = createAndStoreAssignment(start);
			Session.ResetDateForAllAssignmentsAndAudits();

			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			new PersonAssignmentDateSetter().ExecutePersonAssignmentSetterAndWrapInTransaction(Guid.NewGuid(), TimeZoneInfo.Utc);

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(PersonAssignmentDateSetter.DateOfUnconvertedSchedule);
		}

		[Test]
		public void ShouldSetCorrectDayWhenTimeZoneIsCrazy()
		{
			var paRep = new PersonAssignmentRepository(UnitOfWork);
			var start = new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc);

			var pa = createAndStoreAssignment(start);
			Session.ResetDateForAllAssignmentsAndAudits();

			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			new PersonAssignmentDateSetter().ExecutePersonAssignmentSetterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(new DateOnly(2000, 1, 2));
		}

		[Test]
		public void ShouldIncreaseVersionNumber()
		{
			var pa = createAndStoreAssignment(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			var versionBefore = pa.Version;
			new PersonAssignmentDateSetter().ExecutePersonAssignmentSetterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.Utc);
			new PersonAssignmentRepository(UnitOfWork).Get(pa.Id.Value).Version
			                                          .Should().Be.EqualTo(versionBefore + 1);
		}

		private IPersonAssignment createAndStoreAssignment(DateTime start)
		{
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(SetupFixtureForAssembly.loggedOnPerson, new Scenario("d"), new Activity("sdf"), new DateTimePeriod(start, start.AddHours(8)), new ShiftCategory("d"));
			PersistAndRemoveFromUnitOfWork(pa.MainActivities().First().Payload);
			PersistAndRemoveFromUnitOfWork(pa.ShiftCategory);
			PersistAndRemoveFromUnitOfWork(pa.Scenario);
			new PersonAssignmentRepository(UnitOfWork).Add(pa);
			Session.Flush();
			Session.Clear();
			return pa;
		}

	}
}