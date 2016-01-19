using System;
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
			if (timeZone != null)
				person.PermissionInformation.SetDefaultTimeZone(timeZone);

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
			
			person.AddPersonPeriod(new PersonPeriod("2016-01-01".Date(), personContract, team));

			if (terminalDate != null)
				person.TerminatePerson(terminalDate.Date(), new PersonAccountUpdaterDummy());
			_persons.Has(person);
			return this;
		}

	}
}