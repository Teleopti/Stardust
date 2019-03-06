using System;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	public class VerifyAllConvertedTest : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldThrowIfNotFullyConverterted()
		{
			var paRep = PersonAssignmentRepository.DONT_USE_CTOR(CurrUnitOfWork);
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);

			var pa = createAndStoreAssignment(start);

			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(PersonAssignmentDateSetter.DateOfUnconvertedSchedule);
			UnitOfWork.Clear();

			Assert.Throws<NotSupportedException>(() =>
				{
					using (var conn = new SqlConnection(UnitOfWorkFactory.Current.ConnectionString))
					{
						conn.Open();
						using (var tran = conn.BeginTransaction())
						{
							var target = new PersonAssignmentDateSetter();
							target.InjectCheckSqlStatementForTest("select 1");
							target.Execute(tran, pa.Person.Id.Value, TimeZoneInfo.Utc);
							tran.Commit();
						}
					}
				});
		}

		private IPersonAssignment createAndStoreAssignment(DateTime start)
		{
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(SetupFixtureForAssembly.loggedOnPerson, new Scenario("d"), new Activity("sdf"), new DateTimePeriod(start, start.AddHours(8)), new ShiftCategory("d"));
			PersistAndRemoveFromUnitOfWork(pa.MainActivities().First().Payload);
			PersistAndRemoveFromUnitOfWork(pa.ShiftCategory);
			PersistAndRemoveFromUnitOfWork(pa.Scenario);
			PersonAssignmentRepository.DONT_USE_CTOR(CurrUnitOfWork).Add(pa);
			Session.Flush();
			return pa;
		}

	}
}