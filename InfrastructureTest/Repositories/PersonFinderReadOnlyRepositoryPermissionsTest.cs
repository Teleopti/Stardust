using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


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
			var bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams(out var iteams);
			PersistAndRemoveFromUnitOfWork(bu);

			var teams = iteams.ToList();

			var p1 = createPersonsWithTeam(teams[0], "testAgent", "test");
			var p2 = createPersonsWithTeam(teams[1]);
			var p3 = createPersonsWithTeam(teams[0], "Dummy", "dummy");
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
				DefinedRaptorApplicationFunctionForeignIds.PeopleAccess, canSeeUsers, bu.Id.GetValueOrDefault());
			_target.FindPeopleWithDataPermission(searchCriteria);

			searchCriteria.DisplayRows.Select(p => p.FirstName).Should().Contain("dummyUser");
		}

		[Test]
		public void ShouldNotGetAnyResultsWithoutPermission()
		{
			var canSeeUsers = false;
			var bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams(out var iteams);
			PersistAndRemoveFromUnitOfWork(bu);

			var teams = iteams.ToList();

			var p1 = createPersonsWithTeam(teams[0]);
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
				currentUser.Id.GetValueOrDefault(), DefinedRaptorApplicationFunctionForeignIds.PeopleAccess, canSeeUsers, bu.Id.GetValueOrDefault());
			_target.FindPeopleWithDataPermission(searchCriteria);

			searchCriteria.DisplayRows.Count.Should().Be(0);
		}

		[Test]
		public void ShouldOnlyGetAgentsFromSelectedBusinessunit()
		{
			var bu1 = CreateBusinessUnitForTestAndPersist();
			var bu2 = CreateBusinessUnitForTestAndPersist();
			var bu1Teams = bu1.SiteCollection.SelectMany(s => s.TeamCollection).ToList();
			var bu2Teams = bu2.SiteCollection.SelectMany(s => s.TeamCollection).ToList();

			var bu1Person = createPersonsWithTeam(bu1Teams[0], "Magnus", "dummy");
			var bu2Person = createPersonsWithTeam(bu2Teams[0], "Dan", "dummy");
			var bu2Person2 = createPersonsWithTeam(bu2Teams[0], "Emil", "dummy");

			_target.UpdateFindPerson(new [] { bu1Person.Id.GetValueOrDefault(), bu2Person.Id.GetValueOrDefault(), bu2Person2.Id.GetValueOrDefault()});

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
				currentUser.Id.GetValueOrDefault(), DefinedRaptorApplicationFunctionForeignIds.PeopleAccess, true, bu1.Id.GetValueOrDefault());
			_target.FindPeopleWithDataPermission(searchCriteria);

			searchCriteria.DisplayRows.Count.Should().Be(1);
			searchCriteria.DisplayRows.First().FirstName.Equals(bu1Person.Name.FirstName);
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

		private IPerson createPersonsWithTeam(ITeam team, string firstName = "dummyAgent", string lastName = "dummy")
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

		private IBusinessUnit CreateBusinessUnitForTestAndPersist()
		{
			// Teams
			var team1 = TeamFactory.CreateSimpleTeam("Team Raptor");
			var team2 = TeamFactory.CreateSimpleTeam("Team Pro");
			var team3 = TeamFactory.CreateSimpleTeam("Team CCC");

			// Sites
			var danderydUnit = SiteFactory.CreateSimpleSite("Danderyd");
			var strangnasUnit = SiteFactory.CreateSimpleSite("Strangnas");
			danderydUnit.AddTeam(team1);
			danderydUnit.AddTeam(team2);
			strangnasUnit.AddTeam(team3);

			// BusinessUnits
			var swedenBusinessUnit = new BusinessUnit("Sweden");
			swedenBusinessUnit.AddSite(danderydUnit);
			swedenBusinessUnit.AddSite(strangnasUnit);

			PersistAndRemoveFromUnitOfWork(swedenBusinessUnit);
			PersistAndRemoveRangeFromUnitOfWork(danderydUnit, strangnasUnit);
			PersistAndRemoveRangeFromUnitOfWork(team1, team2, team3);

			return swedenBusinessUnit;
		}

		private IPerson createPersonWithoutPersonPeriod(string firstName = "dummyUser", string lastName = "dummy")
		{
			var personToTest = PersonFactory.CreatePerson(new Name(firstName, lastName));
			PersistAndRemoveFromUnitOfWork(personToTest);

			return personToTest;
		}
	}
}
