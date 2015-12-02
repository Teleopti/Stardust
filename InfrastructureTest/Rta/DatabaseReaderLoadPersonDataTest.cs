using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[RtaDatabaseTest]
	public class DatabaseReaderLoadPersonDataTest
	{
		public IDatabaseReader Reader;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository Persons;
		public ISiteRepository Sites;
		public ITeamRepository Teams;
		public IPartTimePercentageRepository PartTimePercentages;
		public IContractRepository Contracts;
		public IContractScheduleRepository ContractSchedules;
		public IExternalLogOnRepository ExternalLogOns;
		public MutableNow Now;

		private IPerson addPerson(int dataSourceId, string externalLogon)
		{
			return addPerson(externalLogon, dataSourceId, new DateOnly(Now.UtcDateTime().Date));
		}

		private IPerson addPerson(string externalLogon, int dataSourceId, DateOnly fromDate)
		{
			var team = TeamFactory.CreateTeam(".", ".");
			var person = PersonFactory.CreatePersonWithPersonPeriod(fromDate);
			var personPeriod = person.Period(fromDate);
			var externalLogOn = new ExternalLogOn
			{
				AcdLogOnOriginalId = externalLogon,
				DataSourceId = dataSourceId
			};
			person.AddExternalLogOn(
				externalLogOn,
				personPeriod
				);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			personPeriod.Team = team;
			WithUnitOfWork.Do(() =>
			{
				ContractSchedules.Add(personPeriod.PersonContract.ContractSchedule);
				Contracts.Add(personPeriod.PersonContract.Contract);
				PartTimePercentages.Add(personPeriod.PersonContract.PartTimePercentage);
				ExternalLogOns.Add(externalLogOn);
				Sites.Add(team.Site);
				Teams.Add(team);
				Persons.Add(person);
			});
			return person;
		}

		private void addPersonPeriod(IPerson person, DateOnly endDate)
		{
			var personPeriod = person.Period(endDate);
			person.AddPersonPeriod(new PersonPeriod(endDate, personPeriod.PersonContract, personPeriod.Team));
			WithUnitOfWork.Do(() => Persons.Add(person));
		}

		[Test]
		public void ShouldFindByExternalLogon()
		{
			var person1 = addPerson(1, "user1");
			var person2 = addPerson(1, "user2");

			var result = Reader.LoadPersonOrganizationData(1, "user2");

			result.Single().PersonId.Should().Be(person2.Id);
		}

		[Test]
		public void ShouldFindNothing()
		{
			addPerson(1, "user");

			var result = Reader.LoadPersonOrganizationData(1, "unknownUser");

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindAllWithSameExternalLogon()
		{
			addPerson(1, "user");
			addPerson(1, "user");

			var result = Reader.LoadPersonOrganizationData(1, "user");

			result.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldFindByActivePersonPeriod()
		{
			Now.Is("2015-12-02");
			var pastUser = addPerson("user", 1, "2015-10-01".Date());
			addPersonPeriod(pastUser, "2015-10-31".Date());
			var currentUser = addPerson("user", 1, "2015-11-01".Date());
			var futureUser = addPerson("user", 1, "2015-12-31".Date());

			var result = Reader.LoadPersonOrganizationData(1, "user");

			result.Single().PersonId.Should().Be(currentUser.Id);
		}

		[Test]
		public void ShouldFindByDataSource()
		{
			var person1 = addPerson(2, "user");
			var person2 = addPerson(3, "user");

			var result = Reader.LoadPersonOrganizationData(2, "user");

			result.Single().PersonId.Should().Be(person1.Id);
		}

		[Test]
		public void ShouldLoadAllStuff()
		{
			var person = addPerson(1, "user");

			var result = Reader.LoadPersonOrganizationData(1, "user");

			result.Single().TeamId.Should().Be(person.PersonPeriodCollection.Single().Team.Id);
			result.Single().SiteId.Should().Be(person.PersonPeriodCollection.Single().Team.Site.Id);
			result.Single().BusinessUnitId.Should().Be(person.PersonPeriodCollection.Single().Team.Site.BusinessUnit.Id);
		}
	}
}