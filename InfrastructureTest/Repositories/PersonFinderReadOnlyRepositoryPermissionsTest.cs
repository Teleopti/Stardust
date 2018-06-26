using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	public class PersonFinderReadOnlyRepositoryPermissionsTest : DatabaseTest
	{
		private IPersonFinderReadOnlyRepository _target;
		private DefinedRaptorApplicationFunctionFactory _factory;
		private IAvailableData _availableData;

		[SetUp]
		public void SetUp()
		{
			_target = new PersonFinderReadOnlyRepository(base.CurrUnitOfWork);
			_factory = new DefinedRaptorApplicationFunctionFactory();
			_availableData = new AvailableData();
		}

		[Test]
		public void ShouldIncludeUsersWithPeopleManageUsersPermission()
		{
			var canSeeUsers = true;

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			var team2 = TeamFactory.CreateTeam("Dummy Site2", "Dummy Team2");

			var p1 = createPersonsWithTeam(team, "testAgent", "test");
			var p2 = createPersonsWithTeam(team2);
			var p3 = createPersonsWithTeam(team, "Dummy", "dummy");
			var p4 = createPersonWithoutPersonPeriod();

			var personList = new Guid[] {p1.Id.GetValueOrDefault(), p2.Id.GetValueOrDefault(), p3.Id.GetValueOrDefault(), p4.Id.GetValueOrDefault() };
			_target.UpdateFindPerson(personList);

			var currentUser = createPersonWithoutPersonPeriod("currentUser", "currentUser");

			var testRole = ApplicationRoleFactory.CreateRole("testRole", "Role with permission");

			createAndAddPermissionsToRole(_factory, testRole);

			PersistAndRemoveFromUnitOfWork(testRole);

			_availableData.ApplicationRole = testRole;
			_availableData.AvailableDataRange = AvailableDataRangeOption.Everyone;
			PersistAndRemoveFromUnitOfWork(_availableData);

			currentUser.PermissionInformation.AddApplicationRole(testRole);

			PersistAndRemoveFromUnitOfWork(currentUser);

			var searchCriteria = new PeoplePersonFinderSearchWithPermissionCriteria(PersonFinderField.All, "dummy", 1, 10,
				new DateOnly(2018, 05, 28), 1, 0, new DateOnly(2018, 05, 28),
				currentUser.Id.GetValueOrDefault(),
				DefinedRaptorApplicationFunctionForeignIds.PeopleAccess, canSeeUsers);
			_target.FindPeopleWithDataPermission(searchCriteria);

			searchCriteria.DisplayRows.Select(p => p.FirstName).Should().Contain("dummyUser");
		}

		[Test]
		public void ShouldNotGetAnyResultsWithoutPermission()
		{
			var canSeeUsers = false;
			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");

			var p1 = createPersonsWithTeam(team);
			var p2 = createPersonWithoutPersonPeriod();

			var personList = new Guid[] { p1.Id.GetValueOrDefault(), p2.Id.GetValueOrDefault() };
			_target.UpdateFindPerson(personList);

			var currentUser = createPersonWithoutPersonPeriod("currentUser", "currentUser");

			var testRole = ApplicationRoleFactory.CreateRole("testRole", "Role with permission");

			PersistAndRemoveFromUnitOfWork(testRole);

			_availableData.ApplicationRole = testRole;
			_availableData.AvailableDataRange = AvailableDataRangeOption.Everyone;
			PersistAndRemoveFromUnitOfWork(_availableData);

			currentUser.PermissionInformation.AddApplicationRole(testRole);

			PersistAndRemoveFromUnitOfWork(currentUser);

			var searchCriteria = new PeoplePersonFinderSearchWithPermissionCriteria(PersonFinderField.All, "dummy", 1, 10,
				new DateOnly(2018, 05, 28), 1, 0, new DateOnly(2018, 05, 28),
				currentUser.Id.GetValueOrDefault(),
				DefinedRaptorApplicationFunctionForeignIds.PeopleAccess, canSeeUsers);
			_target.FindPeopleWithDataPermission(searchCriteria);

			searchCriteria.DisplayRows.Count.Should().Be(0);
		}

		private void createAndAddPermissionsToRole(DefinedRaptorApplicationFunctionFactory factory, ApplicationRole testRole)
		{
			var openRaptor = ApplicationFunction.FindByPath(factory.ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication);

			PersistAndRemoveFromUnitOfWork(openRaptor);

			var anywhere = ApplicationFunction.FindByPath(factory.ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.Anywhere);

			PersistAndRemoveFromUnitOfWork(anywhere);

			var webPeople = ApplicationFunction.FindByPath(factory.ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.WebPeople);

			PersistAndRemoveFromUnitOfWork(webPeople);

			var peopleManageUsers = ApplicationFunction.FindByPath(factory.ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.PeopleManageUsers);

			PersistAndRemoveFromUnitOfWork(peopleManageUsers);

			var peopleAccess = ApplicationFunction.FindByPath(factory.ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.PeopleAccess);

			PersistAndRemoveFromUnitOfWork(peopleAccess);

			testRole.AddApplicationFunction(peopleManageUsers);
			testRole.AddApplicationFunction(peopleAccess);
		}

		private IPerson createPersonsWithTeam(Team team, string firstName = "dummyAgent", string lastName = "dummy")
		{
			var scheduleDate = new DateOnly(2018, 01, 25);

			var personToTest = PersonFactory.CreatePerson(new Name(firstName, lastName));
			PersistAndRemoveFromUnitOfWork(personToTest);

			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(scheduleDate,
				personContract,
				team);
			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateFindPerson(new[] { personToTest.Id.GetValueOrDefault() });

			return personToTest;
		}

		private IPerson createPersonWithoutPersonPeriod(string firstName = "dummyUser", string lastName = "dummy")
		{
			var personToTest = PersonFactory.CreatePerson(new Name(firstName, lastName));
			PersistAndRemoveFromUnitOfWork(personToTest);

			return personToTest;
		}
	}
}
