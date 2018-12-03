using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	public class PersonFinderBug46991Test : DatabaseTest
	{
		private IPersonFinderReadOnlyRepository _target;
		
		[SetUp]
		public void SetUp()
		{
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
		}
		
		[Test]
		public void ShouldHandleRenamedTeam()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			PersistAndRemoveFromUnitOfWork(personToTest);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2017, 1, 1),
				personContract,
				team);

			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);
			
			_target.UpdateFindPerson(new[] {personToTest.Id.Value});

			team.SetDescription(new Description("New team name"));
			PersistAndRemoveFromUnitOfWork(team);
			_target.UpdateFindPersonData(new []{team.Id.GetValueOrDefault()});

			var date = new DateOnly(2017, 4, 1);

			var criteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.Organization,"New",10,date,0,0);
			_target.FindPeople(criteria);
			criteria.TotalRows.Should().Be(1);
		}

		[Test]
		public void ShouldHandleRenamedTeamJustForOrganizationSearchIndex()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			PersistAndRemoveFromUnitOfWork(personToTest);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2017, 1, 1),
				personContract,
				team);

			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateFindPerson(new[] { personToTest.Id.Value });

			team.SetDescription(new Description("New team name"));
			PersistAndRemoveFromUnitOfWork(team);
			_target.UpdateFindPersonData(new[] { team.Id.GetValueOrDefault() });

			var date = new DateOnly(2017, 4, 1);

			var criteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.FirstName, "dummyAgent", 10, date, 0, 0);
			_target.FindPeople(criteria);
			criteria.TotalRows.Should().Be(1);
		}

		[Test]
		public void ShouldHandleRenamedSite()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			PersistAndRemoveFromUnitOfWork(personToTest);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2017, 1, 1),
				personContract,
				team);

			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateFindPerson(new[] { personToTest.Id.Value });

			team.Site.SetDescription(new Description("New site name"));
			PersistAndRemoveFromUnitOfWork(team.Site);
			_target.UpdateFindPersonData(new[] { team.Site.Id.GetValueOrDefault() });

			var date = new DateOnly(2017, 4, 1);

			var criteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.Organization, "New", 10, date, 0, 0);
			_target.FindPeople(criteria);
			criteria.TotalRows.Should().Be(1);
		}

		[Test]
		public void ShouldHandleRenamedSiteJustForOrganizationSearchIndex()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			PersistAndRemoveFromUnitOfWork(personToTest);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2017, 1, 1),
				personContract,
				team);

			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateFindPerson(new[] { personToTest.Id.Value });

			team.Site.SetDescription(new Description("New site name"));
			PersistAndRemoveFromUnitOfWork(team.Site);
			_target.UpdateFindPersonData(new[] { team.Site.Id.GetValueOrDefault() });

			var date = new DateOnly(2017, 4, 1);

			var criteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.FirstName, "dummyAgent", 10, date, 0, 0);
			_target.FindPeople(criteria);
			criteria.TotalRows.Should().Be(1);
		}

		[Test]
		public void ShouldHandleTeamMovedSite()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			PersistAndRemoveFromUnitOfWork(personToTest);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			var newSite = SiteFactory.CreateSimpleSite("New site");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(newSite);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2017, 1, 1),
				personContract,
				team);

			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateFindPerson(new[] { personToTest.Id.Value });

			team.Site = newSite;
			PersistAndRemoveFromUnitOfWork(team);
			_target.UpdateFindPersonData(new[] { team.Id.GetValueOrDefault() });

			var date = new DateOnly(2017, 4, 1);

			var criteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.Organization, "New", 10, date, 0, 0);
			_target.FindPeople(criteria);
			criteria.TotalRows.Should().Be(1);
		}

		[Test]
		public void ShouldHandleTeamMovedSiteForAllIndicies()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			PersistAndRemoveFromUnitOfWork(personToTest);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			var newSite = SiteFactory.CreateSimpleSite("New site");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(newSite);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2017, 1, 1),
				personContract,
				team);

			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateFindPerson(new[] { personToTest.Id.Value });

			team.Site = newSite;
			PersistAndRemoveFromUnitOfWork(team);
			_target.UpdateFindPersonData(new[] { team.Id.GetValueOrDefault() });

			var date = new DateOnly(2017, 4, 1);

			var criteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.FirstName, "dummyAgent", 10, date, 0, 0);
			_target.FindPeople(criteria);
			criteria.DisplayRows[0].SiteId.Should().Be.EqualTo(newSite.Id);
		}
		[Test]
		public void ShouldThrowExceptionWhenTryingToUpdateEntireSearchIndexFromCode()
		{
			Assert.Throws<NotSupportedException>(() => _target.UpdateFindPerson(new[] {Guid.Empty}));
		}
	}
}