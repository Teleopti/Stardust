using System;
using System.Data.SqlClient;
using NHibernate.Envers;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Repositories.Audit;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	[TestFixture]
	public class PersonAssignmentAuditDateSetterTest : AuditTest
	{
		[Test]
		public void ShouldSetCorrectDateForResettedPersonAssignment()
		{
			var target = new PersonAssignmentDateSetter();
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expected = new DateOnly(2000, 1, 1);

			var pa = createAndStoreAssignment(start);

			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			Session.Auditer().Find<PersonAssignment>(pa.Id.Value, 1).Date.Should().Be.EqualTo(AgentDayDateSetter.RestoreDate);
			UnitOfWork.Clear();

			target.Execute(new SqlConnectionStringBuilder(UnitOfWorkFactory.Current.ConnectionString));

			Session.Auditer().Find<PersonAssignment>(pa.Id.Value, 1).Date.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldNotTouchAssignmentIfNotRestoreDate()
		{
			var target = new PersonAssignmentDateSetter();
		
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expected = new DateOnly(2000, 1, 1);

			var pa = createAndStoreAssignment(start);

			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			target.Execute(new SqlConnectionStringBuilder(UnitOfWorkFactory.Current.ConnectionString));

			Session.Auditer().Find<PersonAssignment>(pa.Id.Value, 1).Date.Should().Be.EqualTo(expected);
		}

		private IPersonAssignment createAndStoreAssignment(DateTime start)
		{
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("sdf"),
																																		 SetupFixtureForAssembly.loggedOnPerson,
																																		 new DateTimePeriod(start, start.AddHours(8)),
																																		 new ShiftCategory("d"), new Scenario("d"));
			PersistAndRemoveFromUnitOfWork(pa.MainShift.LayerCollection[0].Payload);
			PersistAndRemoveFromUnitOfWork(pa.MainShift.ShiftCategory);
			PersistAndRemoveFromUnitOfWork(pa.Scenario);
			PersistAndRemoveFromUnitOfWork(pa);
			return pa;
		}
	}
}