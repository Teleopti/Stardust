using System;
using System.Data.SqlClient;
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
			var target = new PersonAssignmentAuditDateSetter();
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

			target.Execute(new SqlConnectionStringBuilder(UnitOfWorkFactory.Current.ConnectionString));

			Session.Auditer().Find<PersonAssignment>(pa.Id.Value, revisions.Last()).Date.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldNotTouchAssignmentIfNotRestoreDate()
		{
			var target = new PersonAssignmentAuditDateSetter();
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expected = new DateOnly(2000, 1, 1);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndStoreAssignment(uow, start);
				uow.PersistAll();
			}

			UnitOfWork.Clear();

			var revisions = Session.Auditer().GetRevisions(typeof(PersonAssignment), pa.Id.Value);

			target.Execute(new SqlConnectionStringBuilder(UnitOfWorkFactory.Current.ConnectionString));

			Session.Auditer().Find<PersonAssignment>(pa.Id.Value, revisions.Last()).Date.Should().Be.EqualTo(expected);
		}

		private void createAndStoreAssignment(IUnitOfWork uow, DateTime start)
		{
			pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("sdf"),
																																		 SetupFixtureForAssembly.loggedOnPerson,
																																		 new DateTimePeriod(start, start.AddHours(8)),
																																		 new ShiftCategory("d"), new Scenario("d"));
			var rep = new Repository(uow);
			rep.Add(pa.MainShift.LayerCollection[0].Payload);
			rep.Add(pa.MainShift.ShiftCategory);
			pa.Scenario.DefaultScenario = true;
			rep.Add(pa.Scenario);
			rep.Add(pa);
		}
	}
}