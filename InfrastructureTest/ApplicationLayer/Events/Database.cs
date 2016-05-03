using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	public class Database
	{
		private readonly IPersonAssignmentRepository _assignments;
		private readonly IPersonRepository _persons;
		private readonly ISiteRepository _sites;
		private readonly ITeamRepository _teams;
		private readonly IContractRepository _contracts;
		private readonly IPartTimePercentageRepository _partTimePercentages;
		private readonly IContractScheduleRepository _contractSchedules;
		private readonly IScenarioRepository _scenarios;
		private readonly IActivityRepository _activities;

		private DateOnly _date;
		private string _person;
		private string _scenario;

		public Database(
			IPersonAssignmentRepository assignments,
			IPersonRepository persons,
			ISiteRepository sites,
			ITeamRepository teams,
			IContractRepository contracts,
			IPartTimePercentageRepository partTimePercentages,
			IContractScheduleRepository contractSchedules,
			IScenarioRepository scenarios, 
			IActivityRepository activities)
		{
			_assignments = assignments;
			_persons = persons;
			_sites = sites;
			_teams = teams;
			_contracts = contracts;
			_partTimePercentages = partTimePercentages;
			_contractSchedules = contractSchedules;
			_scenarios = scenarios;
			_activities = activities;
		}

		[UnitOfWork]
		public virtual Guid CurrentPersonId()
		{
			return person().Id.Value;
		}

		private IPerson person()
		{
			return _persons.LoadAll().Single(x => x.Name.ToString() == _person);
		}

		[UnitOfWork]
		public virtual Guid CurrentScenarioId()
		{
			return scenario().Id.Value;
		}

		private IScenario scenario()
		{
			return _scenarios.LoadAll().Single(x => x.Description.Name == _scenario);
		}

		[UnitOfWork]
		public virtual IPersonAssignment CurrentAssignment()
		{
			return assignment();
		}

		private IPersonAssignment assignment()
		{
			var pa = _assignments.LoadAll().Single(x => x.Date == _date && x.Person == person());
			return _assignments.LoadAggregate(pa.Id.Value);
		}
		
		private IActivity activity(string activity)
		{
			return _activities.LoadAll().Single(x => x.Name.Equals(activity));
		}

		[UnitOfWork]
		public virtual Database WithAgent()
		{
			var person = new Person {Name = new Name(RandomName.Make(), RandomName.Make())};
			_person = person.Name.ToString();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var site = new Site("site");
			var team = new Team {Description = new Description("team")};
			site.AddTeam(team);
			
			_sites.Add(site);
			_teams.Add(team);

			var contract = new Contract("contract");
			_contracts.Add(contract);
			var partTimePercentage = new PartTimePercentage("partTimePercentage");
			_partTimePercentages.Add(partTimePercentage);
			var contractSchedule = new ContractSchedule("contractSchedule");
			_contractSchedules.Add(contractSchedule);

			var personContract = new PersonContract(
				contract, 
				partTimePercentage,
				contractSchedule);

			person.AddPersonPeriod(
				new PersonPeriod("2001-01-01".Date(),
				personContract,
				team));
			_persons.Add(person);
			
			return this;
		}

		[UnitOfWork]
		public virtual Database WithActivity(string name)
		{
			_activities.Add(new Activity(name));
			return this;
		}
		
		[UnitOfWork]
		public virtual Database WithAssignment(DateOnly date)
		{
			var scenario = ensureScenarioExists();
			_date = date;
			_assignments.Add(new PersonAssignment(person(), scenario, date));
			return this;
		}
		
		[UnitOfWork]
		public virtual Database WithLayer(string activityName, DateTime startTime, DateTime endTime)
		{
			startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
			endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);
			assignment().AddActivity(activity(activityName), new DateTimePeriod(startTime, endTime));
			return this;
		}
		
		private IScenario ensureScenarioExists()
		{
			if (_scenario != null)
				return scenario();
			_scenario = RandomName.Make();
			var s = new Scenario(_scenario) { DefaultScenario = true };
			_scenarios.Add(s);
			return s;
		}

	}
}