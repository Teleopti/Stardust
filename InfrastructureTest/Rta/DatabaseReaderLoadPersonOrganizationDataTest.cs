using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
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
	public class DatabaseReaderLoadPersonOrganizationDataTest
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

		private IPerson addPerson(string externalLogon, int dataSourceId)
		{
			return addPerson(externalLogon, dataSourceId, new DateOnly(Now.UtcDateTime().Date));
		}

		private IPerson addPerson(string externalLogon, int dataSourceId, DateOnly fromDate)
		{
			return addPerson(externalLogon, dataSourceId, fromDate, null);
		}

		private IPerson addPerson(string externalLogon, int dataSourceId, DateOnly fromDate, DateOnly? terminalDate)
		{
			return addPerson(externalLogon, dataSourceId, fromDate, terminalDate, null);
		}

		private IPerson addPerson(string externalLogon, int dataSourceId, DateOnly fromDate, DateOnly? terminalDate, TimeZoneInfo timeZone)
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(fromDate);
			person.PermissionInformation.SetDefaultTimeZone(timeZone ?? TimeZoneInfoFactory.UtcTimeZoneInfo());

			var personPeriod = person.Period(fromDate);
			var team = TeamFactory.CreateTeam("team", "site");
			personPeriod.Team = team;

			var externalLogOn = new ExternalLogOn
			{
				AcdLogOnOriginalId = externalLogon,
				DataSourceId = dataSourceId
			};

			person.AddExternalLogOn(
				externalLogOn,
				personPeriod
				);

			if (terminalDate.HasValue)
				person.TerminatePerson(terminalDate.Value, new PersonAccountUpdaterDummy());

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

		private void addPersonPeriod(IPerson person, DateOnly startDate)
		{
			var oldPeriod = person.Period(startDate);

			var newPeriod = new PersonPeriod(startDate, oldPeriod.PersonContract, oldPeriod.Team);
			newPeriod.Team = oldPeriod.Team;
			person.AddPersonPeriod(newPeriod);

			//if (externalLogOn != null)
			//	person.AddExternalLogOn(externalLogOn, newPeriod);

			WithUnitOfWork.Do(() =>
			{
				Persons.Add(person);
			});
		}
		
		[Test]
		public void ShouldFindByExternalLogon()
		{
			var person1 = addPerson("user1", 1);
			var person2 = addPerson("user2", 1);

			var result = Reader.LoadPersonOrganizationData(1, "user2");

			result.Single().PersonId.Should().Be(person2.Id);
		}

		[Test]
		public void ShouldFindNothing()
		{
			addPerson("user", 1);

			var result = Reader.LoadPersonOrganizationData(1, "unknownUser");

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindAllWithSameExternalLogon()
		{
			addPerson("user", 1);
			addPerson("user", 1);

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
			var person1 = addPerson("user", 2);
			var person2 = addPerson("user", 3);

			var result = Reader.LoadPersonOrganizationData(2, "user");

			result.Single().PersonId.Should().Be(person1.Id);
		}

		[Test]
		public void ShouldLoadProperties()
		{
			var person = addPerson("user", 1);

			var result = Reader.LoadPersonOrganizationData(1, "user");

			result.Single().TeamId.Should().Be(person.PersonPeriodCollection.Single().Team.Id);
			result.Single().SiteId.Should().Be(person.PersonPeriodCollection.Single().Team.Site.Id);
			result.Single().BusinessUnitId.Should().Be(person.PersonPeriodCollection.Single().Team.Site.BusinessUnit.Id);
		}

		[Test]
		public void ShouldNotFindTerminatedPersons()
		{
			Now.Is("2016-01-20 00:00");
			var person = addPerson("user", 1, "2016-01-01".Date(), "2016-01-19".Date());

			var result = Reader.LoadPersonOrganizationData(1, "user");

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindTerminatedPersonsTheirLastDay()
		{
			Now.Is("2016-01-20 08:00");
			var person = addPerson("user", 1, "2016-01-01".Date(), "2016-01-20".Date());

			var result = Reader.LoadPersonOrganizationData(1, "user");

			result.Single().PersonId.Should().Be(person.Id.Value);
		}

		[Test]
		public void ShouldNotFindTerminatedPersonsInInstanbul()
		{
			Now.Is("2016-01-20 23:00");
			var person = addPerson("user", 1, "2016-01-01".Date(), "2016-01-20".Date(), TimeZoneInfoFactory.IstanbulTimeZoneInfo());

			var result = Reader.LoadPersonOrganizationData(1, "user");

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindTerminatedPersonsInHawaiiTheirLastHour()
		{
			Now.Is("2016-01-21 09:00");
			var person = addPerson("user", 1, "2016-01-01".Date(), "2016-01-20".Date(), TimeZoneInfoFactory.HawaiiTimeZoneInfo());

			var result = Reader.LoadPersonOrganizationData(1, "user");

			result.Single().PersonId.Should().Be(person.Id.Value);
		}
		
	}
}