using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public static class FakeDatabaseAgentExtensions
	{

		public static FakeDatabase WithAgent(this FakeDatabase database, string name)
		{
			return database.WithAgent(null, name, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid id, string name)
		{
			return database.WithAgent(id, name, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate)
		{
			return database.WithAgent(null, name, terminalDate, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate, Guid teamId)
		{
			return database.WithAgent(null, name, terminalDate, teamId, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid id, string name, string terminalDate, Guid teamId)
		{
			return database.WithAgent(id, name, terminalDate, teamId, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid id, string name, string terminalDate)
		{
			return database.WithAgent(id, name, terminalDate, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, TimeZoneInfo timeZone)
		{
			return database.WithAgent(null, name, null, null, timeZone);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate, TimeZoneInfo timeZone)
		{
			return database.WithAgent(null, name, terminalDate, null, timeZone);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid id, string name, TimeZoneInfo timeZone)
		{
			return database.WithAgent(id, name, null, null, timeZone);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate, Guid? teamId, TimeZoneInfo timeZone)
		{
			database.WithPerson(id, name, terminalDate, timeZone);
			database.WithPeriod("2016-01-01", teamId);
			return database;
		}
	}

	public static class FakeDatabasePeriodExtensions
	{
		public static FakeDatabase WithPeriod(this FakeDatabase database, string startDate)
		{
			return database.WithPeriod(startDate, null, null, null);
		}

		public static FakeDatabase WithPeriod(this FakeDatabase database, string startDate, Guid? teamId)
		{
			return database.WithPeriod(startDate, teamId, null, null);
		}

		public static FakeDatabase WithPeriod(this FakeDatabase database, string startDate, Guid? teamId, Guid? siteId)
		{
			return database.WithPeriod(startDate, teamId, siteId, null);
		}
	}

	public static class FakeDatabasePersonExtensions
	{
		public static FakeDatabase WithPerson(this FakeDatabase database, string name)
		{
			return database.WithPerson(null, name, null, null);
		}
	}

	public class FakeDatabase
	{
		private readonly FakePersonRepository _persons;
		private readonly FakeBusinessUnitRepository _businessUnits;
		private readonly FakeSiteRepository _sites;
		private readonly FakeTeamRepository _teams;
		private readonly FakeContractRepository _contracts;
		private readonly FakePartTimePercentageRepository _partTimePercentages;
		private readonly FakeContractScheduleRepository _contractSchedules;

		public FakeDatabase(
			FakePersonRepository persons, 
			FakeBusinessUnitRepository businessUnits,
			FakeSiteRepository sites,
			FakeTeamRepository teams,
			FakeContractRepository contracts,
			FakePartTimePercentageRepository partTimePercentages,
			FakeContractScheduleRepository contractSchedules
			)
		{
			_persons = persons;
			_businessUnits = businessUnits;
			_sites = sites;
			_teams = teams;
			_contracts = contracts;
			_partTimePercentages = partTimePercentages;
			_contractSchedules = contractSchedules;
		}


		public FakeDatabase WithPerson(Guid? id, string name, string terminalDate, TimeZoneInfo timeZone)
		{
			var person = new Person { Name = new Name(name, "") };
			person.SetId(id ?? Guid.NewGuid());
			_persons.Has(person);

			if (timeZone != null)
				person.PermissionInformation.SetDefaultTimeZone(timeZone);

			if (terminalDate != null)
				person.TerminatePerson(terminalDate.Date(), new PersonAccountUpdaterDummy());

			return this;
		}

		public FakeDatabase WithPeriod(string startDate, Guid? teamId, Guid? siteId, Guid? businessUnitId)
		{
			var businessUnit = new BusinessUnit("b");
			businessUnit.SetId(businessUnitId ?? Guid.NewGuid());
			_businessUnits.Has(businessUnit);

			var site = new Site("s");
			site.SetBusinessUnit(businessUnit);
			site.SetId(siteId ?? Guid.NewGuid());
			_sites.Has(site);

			var team = new Team();
			team.Site = site;
			team.SetId(teamId ?? Guid.NewGuid());
			_teams.Has(team);

			var contract = new Contract("c");
			contract.SetId(Guid.NewGuid());
			_contracts.Has(contract);

			var partTimePercentage = new PartTimePercentage("p");
			partTimePercentage.SetId(Guid.NewGuid());
			_partTimePercentages.Has(partTimePercentage);

			var contractSchedule = new ContractSchedule("cs");
			contractSchedule.SetId(Guid.NewGuid());
			_contractSchedules.Has(contractSchedule);
			var personContract = new PersonContract(contract, partTimePercentage, contractSchedule);

			var person = _persons.LoadAll().Last();

			person.AddPersonPeriod(new PersonPeriod(startDate.Date(), personContract, team));

			return this;
		}
	}
}