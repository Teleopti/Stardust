using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	public class VerifyAllConvertedTest : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldThrowIfNotFullyConverterted()
		{
			var paRep = new PersonAssignmentRepository(UnitOfWork);
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);

			var pa = createAndStoreAssignment(start);

			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(AgentDayConverters.DateOfUnconvertedSchedule);
			UnitOfWork.Clear();

			Assert.Throws<NotSupportedException>(() =>
				{
					using (var conn = new SqlConnection(UnitOfWorkFactory.Current.ConnectionString))
					{
						conn.Open();
						using (var tran = conn.BeginTransaction())
						{
							var target = new dontFixDates();
							target.Execute(tran, pa.Person.Id.Value, TimeZoneInfo.Utc);
							tran.Commit();
						}
					}
				});
		}

		private IPersonAssignment createAndStoreAssignment(DateTime start)
		{
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("sdf"),
																																		 SetupFixtureForAssembly.loggedOnPerson,
																																		 new DateTimePeriod(start, start.AddHours(8)),
																																		 new ShiftCategory("d"), new Scenario("d"));
			PersistAndRemoveFromUnitOfWork(pa.MainLayers().First().Payload);
			PersistAndRemoveFromUnitOfWork(pa.ShiftCategory);
			PersistAndRemoveFromUnitOfWork(pa.Scenario);
			PersistAndRemoveFromUnitOfWork(pa);
			return pa;
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var s = fetchSession(uow);
				s.CreateQuery("update Activity set IsDeleted=1").ExecuteUpdate();
				s.CreateQuery("update ShiftCategory set IsDeleted=1").ExecuteUpdate();
				s.CreateQuery("update Scenario set IsDeleted=1").ExecuteUpdate();
				uow.PersistAll();
			}
		}

		private static ISession fetchSession(IUnitOfWork uow)
		{
			return (ISession)typeof(NHibernateUnitOfWork).GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic)
															.GetValue(uow, null);
		}

		private class dontFixDates : PersonAssignmentDateSetter
		{
			protected override string NumberOfNotConvertedCommand
			{
				get { return "select 1"; }
			}
		}
	}
}