using System;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	[TestFixture]
	public class RemoveEmptyAssignmentsTest : DatabaseTestWithoutTransaction
	{
		private IScenario scenario;

		[Test]
		public void ShouldRemoveAssignmentOnUnsetDate()
		{
			var assShouldBeRemoved = new PersonAssignment(SetupFixtureForAssembly.loggedOnPerson, scenario, AgentDayConverters.DateOfUnconvertedSchedule);
			PersistAndRemoveFromUnitOfWork(assShouldBeRemoved);

			new RemoveEmptyAssignments().ExecuteConverterAndWrapInTransaction(Guid.NewGuid(), null);

			new PersonAssignmentRepository(UnitOfWork).Get(assShouldBeRemoved.Id.Value)
				.Should().Be.Null();
		}

		[Test]
		public void ShouldNotRemoveAssignmentOnNormalDateEvenIfItsEmpty()
		{
			var assShouldNotBeRemoved = new PersonAssignment(SetupFixtureForAssembly.loggedOnPerson, scenario, new DateOnly(2000,1,1));
			PersistAndRemoveFromUnitOfWork(assShouldNotBeRemoved);

			new RemoveEmptyAssignments().ExecuteConverterAndWrapInTransaction(Guid.NewGuid(), null);

			new PersonAssignmentRepository(UnitOfWork).Get(assShouldNotBeRemoved.Id.Value)
				.Should().Be.EqualTo(assShouldNotBeRemoved);
		}

		protected override void SetupForRepositoryTestWithoutTransaction()
		{
			scenario = new Scenario("asdf");
			PersistAndRemoveFromUnitOfWork(scenario);
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var s = fetchSession(uow);
				s.CreateQuery("update Scenario set IsDeleted=1").ExecuteUpdate();
				s.CreateQuery("delete from PersonAssignment").ExecuteUpdate();
				uow.PersistAll();
			}
		}

		private static ISession fetchSession(IUnitOfWork uow)
		{
			return (ISession)typeof(NHibernateUnitOfWork).GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic)
															.GetValue(uow, null);
		}
	}
}