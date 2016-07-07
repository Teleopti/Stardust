using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[MultiDatabaseTest]
	public class DatabaseReaderLoadAllPersonOrganizationDataTest
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

		private IPerson addPerson(string externalLogon, int dataSourceId, DateOnly fromDate, DateOnly? terminalDate,
			TimeZoneInfo timeZone)
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
		public void ShouldLoadAllData()
		{
			var person1 = addPerson("user", 2);
			var person2 = addPerson("user", 3);

			var result = WithUnitOfWork.Get(() => Reader.LoadAllPersonOrganizationData());

			result.Select(r => r.PersonId).Should().Have.SameValuesAs(person1.Id.Value, person2.Id.Value);
		}

		[Test]
		public void ShouldFindAllByActivePersonPeriod()
		{
			Now.Is("2015-12-02");
			var pastUser = addPerson("user", 1, "2015-10-01".Date());
			addPersonPeriod(pastUser, "2015-10-31".Date());
			var currentUser = addPerson("user", 1, "2015-11-01".Date());
			var futureUser = addPerson("user", 1, "2015-12-31".Date());

			var result = WithUnitOfWork.Get(() => Reader.LoadAllPersonOrganizationData());

			result.Single().PersonId.Should().Be(currentUser.Id);
		}

		[Test]
		public void ShouldLoadAllWithProperties()
		{
			var person = addPerson("user", 1);

			var results = WithUnitOfWork.Get(() => Reader.LoadAllPersonOrganizationData());

			var result = results.Single(r => r.PersonId == person.Id);
			result.TeamId.Should().Be(person.PersonPeriodCollection.Single().Team.Id);
			result.SiteId.Should().Be(person.PersonPeriodCollection.Single().Team.Site.Id);
			result.BusinessUnitId.Should().Be(person.PersonPeriodCollection.Single().Team.Site.BusinessUnit.Id);
		}


		[Test]
		public void ShouldNotFindTerminatedPersons()
		{
			Now.Is("2016-01-20 00:00");
			var person = addPerson("user", 1, "2016-01-01".Date(), "2016-01-19".Date());

			var result = WithUnitOfWork.Get(() => Reader.LoadAllPersonOrganizationData());

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindTerminatedPersonsTheirLastDay()
		{
			Now.Is("2016-01-20 08:00");
			var person = addPerson("user", 1, "2016-01-01".Date(), "2016-01-20".Date());

			var result = WithUnitOfWork.Get(() => Reader.LoadAllPersonOrganizationData());

			result.Single().PersonId.Should().Be(person.Id.Value);
		}

		[Test]
		public void ShouldNotFindTerminatedPersonsInInstanbul()
		{
			Now.Is("2016-01-20 23:00");
			var person = addPerson("user", 1, "2016-01-01".Date(), "2016-01-20".Date(),
				TimeZoneInfoFactory.IstanbulTimeZoneInfo());

			var result = WithUnitOfWork.Get(() => Reader.LoadAllPersonOrganizationData());

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindTerminatedPersonsInHawaiiTheirLastHour()
		{
			Now.Is("2016-01-21 09:00");
			var person = addPerson("user", 1, "2016-01-01".Date(), "2016-01-20".Date(), TimeZoneInfoFactory.HawaiiTimeZoneInfo());

			var result = WithUnitOfWork.Get(() => Reader.LoadAllPersonOrganizationData());

			result.Single().PersonId.Should().Be(person.Id.Value);

		}

	}
}