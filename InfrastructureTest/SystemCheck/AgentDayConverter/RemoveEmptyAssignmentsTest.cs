using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Interfaces.Domain;

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
				var s = uow.FetchSession();
				s.CreateQuery("update Scenario set IsDeleted=1").ExecuteUpdate();
				s.CreateQuery("delete from PersonAssignment").ExecuteUpdate();
				uow.PersistAll();
			}
		}
	}
}