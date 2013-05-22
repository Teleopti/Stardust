using System;
using System.Linq;
using NHibernate.Envers;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Repositories.Audit;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	[TestFixture]
	public class PersonAssignmentAuditDateSetterTest : AuditTest
	{
		IPersonAssignment pa;

		[Test]
		public void ShouldSetCorrectDateForResettedPersonAssignment()
		{
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expected = new DateOnly(2000, 1, 1);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndStoreAssignment(uow, start);
				uow.PersistAll();
			}
			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.Clear();

			var revisions = Session.Auditer().GetRevisions(typeof(PersonAssignment), pa.Id.Value);
			new PersonAssignmentAuditDateSetter().ExecuteConverterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.Utc);

			Session.Auditer().Find<PersonAssignment>(pa.Id.Value, revisions.Last()).Date.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldNotTouchAssignmentIfNotRestoreDate()
		{
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expected = new DateOnly(2000, 1, 1);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndStoreAssignment(uow, start);
				uow.PersistAll();
			}

			UnitOfWork.Clear();

			var revisions = Session.Auditer().GetRevisions(typeof(PersonAssignment), pa.Id.Value);

			new PersonAssignmentAuditDateSetter().ExecuteConverterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.Utc);

			Session.Auditer().Find<PersonAssignment>(pa.Id.Value, revisions.Last()).Date.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldNotConvertWrongPerson()
		{
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndStoreAssignment(uow, start);
				uow.PersistAll();
			}
			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.Clear();

			var revisions = Session.Auditer().GetRevisions(typeof(PersonAssignment), pa.Id.Value);

			new PersonAssignmentAuditDateSetter().ExecuteConverterAndWrapInTransaction(Guid.NewGuid(), TimeZoneInfo.Utc);

			Session.Auditer().Find<PersonAssignment>(pa.Id.Value, revisions.Last()).Date.Should().Be.EqualTo(AgentDayConverters.DateOfUnconvertedSchedule);
		}

		[Test]
		public void ShouldSetCorrectDayWhenTimeZoneIsCrazy()
		{
			var start = new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndStoreAssignment(uow, start);
				uow.PersistAll();
			}
			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.Clear();

			var revisions = Session.Auditer().GetRevisions(typeof(PersonAssignment), pa.Id.Value);
			new PersonAssignmentAuditDateSetter().ExecuteConverterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

			Session.Auditer()
			       .Find<PersonAssignment>(pa.Id.Value, revisions.Last())
			       .Date.Should().Be.EqualTo(new DateOnly(2000, 1, 2));
		}

		[Test]
		public void ShouldIncreaseVersionNumber()
		{
			var start = new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndStoreAssignment(uow, start);
				uow.PersistAll();
			}
			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.Clear();

			var revisions = Session.Auditer().GetRevisions(typeof(PersonAssignment), pa.Id.Value);
			var versionBefore = Session.Auditer().Find<PersonAssignment>(pa.Id.Value, revisions.Last()).Version;
			new PersonAssignmentAuditDateSetter().ExecuteConverterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.Utc);

			Session.Clear();
			Session.Auditer()
						 .Find<PersonAssignment>(pa.Id.Value, revisions.Last())
						 .Version.Should().Be.EqualTo(versionBefore + 1);
		}

		private void createAndStoreAssignment(IUnitOfWork uow, DateTime start)
		{
			pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("sdf"),
																																		 SetupFixtureForAssembly.loggedOnPerson,
																																		 new DateTimePeriod(start, start.AddHours(8)),
																																		 new ShiftCategory("d"), new Scenario("d"));
			var rep = new Repository(uow);
			rep.Add(pa.ToMainShift().LayerCollection[0].Payload);
			rep.Add(pa.ShiftCategory);
			pa.Scenario.DefaultScenario = true;
			rep.Add(pa.Scenario);
			rep.Add(pa);
		}
	}
}