using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
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

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid id, string name, string terminalDate,
			Guid teamId)
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

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate,
			TimeZoneInfo timeZone)
		{
			return database.WithAgent(null, name, terminalDate, null, timeZone);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid id, string name, TimeZoneInfo timeZone)
		{
			return database.WithAgent(id, name, null, null, timeZone);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate,
			Guid? teamId, TimeZoneInfo timeZone)
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

		private BusinessUnit _businessUnit;
		private Site _site;
		private Person _person;
		private Team _team;
		private Contract _contract;
		private PartTimePercentage _partTimePercentage;
		private ContractSchedule _contractSchedule;

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

		public FakeDatabase WithBusinessUnit(Guid? id)
		{
			_businessUnit = new BusinessUnit("b");
			_businessUnit.SetId(id ?? Guid.NewGuid());
			_businessUnits.Has(_businessUnit);
			return this;
		}

		public FakeDatabase WithSite(Guid? id)
		{
			_site = new Site("s");
			_site.SetBusinessUnit(_businessUnit);
			_site.SetId(id ?? Guid.NewGuid());
			_sites.Has(_site);
			return this;
		}

		public FakeDatabase WithTeam(Guid? id)
		{
			_team = new Team {Site = _site};
			_team.SetId(id ?? Guid.NewGuid());
			_teams.Has(_team);
			return this;
		}

		public FakeDatabase WithContract(Guid? id)
		{
			_contract = new Contract("c");
			_contract.SetBusinessUnit(_businessUnit);
			_contract.SetId(id ?? Guid.NewGuid());
			_contracts.Has(_contract);
			return this;
		}

		public FakeDatabase WithPartTimePercentage(Guid? id)
		{
			_partTimePercentage = new PartTimePercentage("p");
			_partTimePercentage.SetBusinessUnit(_businessUnit);
			_partTimePercentage.SetId(Guid.NewGuid());
			_partTimePercentages.Has(_partTimePercentage);
			return this;
		}

		public FakeDatabase WithContractSchedule(Guid? id)
		{
			_contractSchedule = new ContractSchedule("cs");
			_contractSchedule.SetBusinessUnit(_businessUnit);
			_contractSchedule.SetId(Guid.NewGuid());
			_contractSchedules.Has(_contractSchedule);
			return this;
		}



		public FakeDatabase WithPerson(Guid? id, string name, string terminalDate, TimeZoneInfo timeZone)
		{
			_person = new Person {Name = new Name(name, "")};
			_person.SetId(id ?? Guid.NewGuid());
			_persons.Has(_person);

			if (timeZone != null)
				_person.PermissionInformation.SetDefaultTimeZone(timeZone);

			if (terminalDate != null)
				_person.TerminatePerson(terminalDate.Date(), new PersonAccountUpdaterDummy());

			return this;
		}

		public FakeDatabase WithPeriod(string startDate, Guid? teamId, Guid? siteId, Guid? businessUnitId)
		{
			ensureBusinssUnit(businessUnitId);
			ensureSite(siteId);
			ensureTeam(teamId);

			ensureContract(null);
			ensurePartTimePercentage(null);
			ensureContractSchedule(null);

			var personContract = new PersonContract(_contract, _partTimePercentage, _contractSchedule);
			_person.AddPersonPeriod(new PersonPeriod(startDate.Date(), personContract, _team));

			return this;
		}


		private void ensureBusinssUnit(Guid? id)
		{
			ensureAggregate(id, () => _businessUnits.LoadAll(), i => WithBusinessUnit(i));
		}

		private void ensureSite(Guid? id)
		{
			ensureAggregate(id, () => _sites.LoadAll(), i => WithSite(i));
		}

		private void ensureTeam(Guid? id)
		{
			ensureAggregate(id, () => _teams.LoadAll(), i => WithTeam(i));
		}

		private void ensureContract(Guid? id)
		{
			ensureAggregate(id, () => _contracts.LoadAll(), i => WithContract(i));
		}

		private void ensurePartTimePercentage(Guid? id)
		{
			ensureAggregate(id, () => _partTimePercentages.LoadAll(), i => WithPartTimePercentage(i));
		}

		private void ensureContractSchedule(Guid? id)
		{
			ensureAggregate(id, () => _contractSchedules.LoadAll(), i => WithContractSchedule(i));
		}

		private static void ensureAggregate<T>(Guid? id, Func<IEnumerable<T>> loadAggregates, Action<Guid?> withAggregate)
			where T : IAggregateRoot
		{
			if (id.HasValue)
			{
				var existing = loadAggregates().SingleOrDefault(x => x.Id.Equals(id));
				if (existing != null)
					return;
				withAggregate(id);
			}
			if (loadAggregates().IsEmpty())
				withAggregate(id);
		}
	}
}