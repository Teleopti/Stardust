using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeDatabase
	{
		private readonly FakePersonRepository _persons;
		private readonly FakeTeamRepository _teams;
		private readonly FakeContractRepository _contracts;
		private readonly FakePartTimePercentageRepository _partTimePercentages;
		private readonly FakeContractScheduleRepository _contractSchedules;

		public FakeDatabase(
			FakePersonRepository persons, 
			FakeTeamRepository teams,
			FakeContractRepository contracts,
			FakePartTimePercentageRepository partTimePercentages,
			FakeContractScheduleRepository contractSchedules
			)
		{
			_persons = persons;
			_teams = teams;
			_contracts = contracts;
			_partTimePercentages = partTimePercentages;
			_contractSchedules = contractSchedules;
		}

		public FakeDatabase HasPerson(string name)
		{
			return hasPerson(null, name, null, null, null);
		}

		public FakeDatabase HasPerson(string name, string terminalDate)
		{
			return hasPerson(null, name, terminalDate, null, null);
		}

		public FakeDatabase HasPerson(string name, string terminalDate, Guid teamId)
		{
			return hasPerson(null, name, terminalDate, teamId, null);
		}

		public FakeDatabase HasPerson(Guid id, string name, string terminalDate, Guid teamId)
		{
			return hasPerson(id, name, terminalDate, teamId, null);
		}

		public FakeDatabase HasPerson(Guid id, string name, string terminalDate)
		{
			return hasPerson(id, name, terminalDate, null, null);
		}

		public FakeDatabase HasPerson(string name, string terminalDate, TimeZoneInfo timeZone)
		{
			return hasPerson(null, name, terminalDate, null, timeZone);
		}

		public FakeDatabase HasPerson(Guid personId, string name, TimeZoneInfo timeZone)
		{
			return hasPerson(personId, name, null, null, timeZone);
		}

		private FakeDatabase hasPerson(Guid? id, string name, string terminalDate, Guid? teamId, TimeZoneInfo timeZone)
		{
			var person = new Person { Name = new Name(name, "") };
			person.SetId(id ?? Guid.NewGuid());
			_persons.Has(person);

			if (timeZone != null)
				person.PermissionInformation.SetDefaultTimeZone(timeZone);

			WithPeriod("2016-01-01", teamId);

			if (terminalDate != null)
				person.TerminatePerson(terminalDate.Date(), new PersonAccountUpdaterDummy());

			return this;
		}

		public FakeDatabase WithPeriod(string startDate)
		{
			return WithPeriod(startDate, null);
		}

		public FakeDatabase WithPeriod(string startDate, Guid? teamId)
		{
			var team = new Team();
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