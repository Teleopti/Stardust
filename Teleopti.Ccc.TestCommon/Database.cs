using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
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
		private string _site;
		private string _team;
		private string _contract;
		private string _partTimePercentage;
		private string _contractSchedule;

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

		[UnitOfWork]
		public virtual Guid CurrentScenarioId()
		{
			return scenario().Id.Value;
		}

		[UnitOfWork]
		public virtual IPersonAssignment CurrentAssignment()
		{
			return assignment();
		}
		
		public virtual Database WithPerson(string name)
		{
			_person = name;
			return this;
		}

		[UnitOfWork]
		public virtual Database WithAgent()
		{
			var person = new Person {Name = new Name(RandomName.Make(), RandomName.Make())};
			_person = person.Name.ToString();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			
			var personContract = new PersonContract(
				contract(), 
				partTimePercentage(),
				contractSchedule());

			person.AddPersonPeriod(
				new PersonPeriod("2001-01-01".Date(),
				personContract,
				team()));
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
			_date = date;
			_assignments.Add(new PersonAssignment(person(), scenario(), date));
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

		private ISite site()
		{
			if (_site != null)
				return _sites.LoadAll().Single(x => x.Description.Name == _site);
			_site = RandomName.Make();
			var s = new Site(_site);
			_sites.Add(s);
			return s;
		}

		private ITeam team()
		{
			if (_team != null)
				return _teams.LoadAll().Single(x => x.Description.Name == _team);
			_team = RandomName.Make();
			var s = site();
			var t = new Team
			{
				Description = new Description(_team),
				Site = s
			};
			_teams.Add(t);
			s.AddTeam(t);
			return t;
		}

		private IContractSchedule contractSchedule()
		{
			if (_contractSchedule != null)
				return _contractSchedules.LoadAll().Single(x => x.Description.Name == _contractSchedule);
			_contractSchedule = RandomName.Make();
			var c = new ContractSchedule(_contractSchedule);
			_contractSchedules.Add(c);
			return c;
		}

		private IPartTimePercentage partTimePercentage()
		{
			if (_partTimePercentage != null)
				return _partTimePercentages.LoadAll().Single(x => x.Description.Name == _partTimePercentage);
			_partTimePercentage = RandomName.Make();
			var p = new PartTimePercentage(_partTimePercentage);
			_partTimePercentages.Add(p);
			return p;
		}

		private IContract contract()
		{
			if (_contract != null)
				return _contracts.LoadAll().Single(x => x.Description.Name == _contract);
			_contract = RandomName.Make();
			var c = new Contract(_contract);
			_contracts.Add(c);
			return c;
		}

		private IScenario scenario()
		{
			if (_scenario != null)
				return _scenarios.LoadAll().Single(x => x.Description.Name == _scenario);
			_scenario = RandomName.Make();
			var s = new Scenario(_scenario) { DefaultScenario = true };
			_scenarios.Add(s);
			return s;
		}

		private IPerson person()
		{
			return _persons.LoadAll().Single(x => x.Name.ToString() == _person);
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

	}
}