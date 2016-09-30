using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class Database
	{
		private readonly AnalyticsDatabase _analytics;
		private readonly IEventPublisher _eventPublisher;
		private readonly IPersonAssignmentRepository _assignments;
		private readonly IPersonRepository _persons;
		private readonly ISiteRepository _sites;
		private readonly ITeamRepository _teams;
		private readonly IContractRepository _contracts;
		private readonly IPartTimePercentageRepository _partTimePercentages;
		private readonly IContractScheduleRepository _contractSchedules;
		private readonly IScenarioRepository _scenarios;
		private readonly IActivityRepository _activities;
		private readonly ISkillRepository _skills;
		private readonly ISkillTypeRepository _skillTypes;
		private readonly IRtaStateGroupRepository _stateGroups;
		private readonly IRtaRuleRepository _rules;
		private readonly IRtaMapRepository _mappings;
		private readonly IExternalLogOnRepository _externalLogOns;

		private DateOnly _date;
		private string _person;
		private string _scenario;
		private string _site;
		private string _team;
		private string _contract;
		private string _partTimePercentage;
		private string _contractSchedule;
		private string _stateGroup;
		private string _activity;
		private string _rule;

		public Database(
			AnalyticsDatabase analytics,
			IEventPublisher eventPublisher,
			IPersonAssignmentRepository assignments,
			IPersonRepository persons,
			ISiteRepository sites,
			ITeamRepository teams,
			IContractRepository contracts,
			IPartTimePercentageRepository partTimePercentages,
			IContractScheduleRepository contractSchedules,
			IScenarioRepository scenarios, 
			IActivityRepository activities, 
			ISkillRepository skills, 
			ISkillTypeRepository skillTypes,
			IRtaStateGroupRepository stateGroups,
			IRtaRuleRepository rules,
			IRtaMapRepository mappings,
			IExternalLogOnRepository externalLogOns
			)
		{
			_analytics = analytics;
			_eventPublisher = eventPublisher;
			_assignments = assignments;
			_persons = persons;
			_sites = sites;
			_teams = teams;
			_contracts = contracts;
			_partTimePercentages = partTimePercentages;
			_contractSchedules = contractSchedules;
			_scenarios = scenarios;
			_activities = activities;
			_skills = skills;
			_skillTypes = skillTypes;
			_stateGroups = stateGroups;
			_rules = rules;
			_mappings = mappings;
			_externalLogOns = externalLogOns;
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

		[UnitOfWork]
		public virtual Guid SkillIdFor(string name)
		{
			return _skills.LoadAll().Where(x => x.Name == name).Select(x => x.Id).Single().Value;
		}
		
		[UnitOfWork]
		public virtual Guid PersonIdFor(string name)
		{
			return _persons.LoadAll().Single(x => x.Name == new Name(name, name)).Id.Value;
		}






		[UnitOfWork]
		public virtual Database WithDefaultScenario(string name)
		{
			var scenario = new Scenario(name) {DefaultScenario = true, EnableReporting = true};
			_scenario = scenario.Description.Name;
			_scenarios.Add(scenario);
			return this;
		}

		private IScenario scenario()
		{
			if (_scenario != null)
				return _scenarios.LoadAll().Single(x => x.Description.Name == _scenario);
			_scenario = RandomName.Make();
			var s = new Scenario(_scenario) { DefaultScenario = true, EnableReporting = true };
			_scenarios.Add(s);
			return s;
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

		[UnitOfWork]
		public virtual Database WithPerson(string name)
		{
			var person = new Person { Name = new Name(name, name) };
			_person = person.Name.ToString();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			_persons.Add(person);
			return this;
		}

		[UnitOfWork]
		public virtual Database WithPersonPeriod(DateOnly date)
		{
			var personContract = new PersonContract(
				contract(),
				partTimePercentage(),
				contractSchedule());

			person().AddPersonPeriod(
				new PersonPeriod(date,
				personContract,
				team()));
			
			return this;
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

		[UnitOfWork]
		public virtual Database WithAgent()
		{
			return WithAgent(RandomName.Make());
		}

		[UnitOfWork]
		public virtual Database WithAgent(string name)
		{
			var existing = _persons.LoadAll().SingleOrDefault(x => x.Name.FirstName == name);
			if (existing != null)
			{
				_person = new Name(name, name).ToString();
				return this;
			}

			var person = new Person { Name = new Name(name, name) };
			_person = person.Name.ToString();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var personContract = new PersonContract(
				contract(),
				partTimePercentage(),
				contractSchedule());

			var personPeriod = new PersonPeriod("2001-01-01".Date(), personContract, team());
			person.AddPersonPeriod(personPeriod);

			var exteralLogOn = new ExternalLogOn
			{
				//AcdLogOnName = name, // is not used?
				AcdLogOnMartId = -1, // NotDefined should be there, 0 probably wont
				DataSourceId = _analytics.CurrentDataSourceId,
				AcdLogOnOriginalId = name // this is what the rta receives
			};
			_externalLogOns.Add(exteralLogOn);
			person.AddExternalLogOn(exteralLogOn, personPeriod);

			_persons.Add(person);

			return this;
		}
		
		private IPerson person()
		{
			return _persons.LoadAll().Single(x => x.Name.ToString() == _person);
		}










		[UnitOfWork]
		public virtual Database WithSkill(string name)
		{
			var skill = withSkill(name);

			var personSkill = new PersonSkill(skill, new Percent(100));
			person().AddSkill(personSkill, person().PersonPeriodCollection.OrderBy(x => x.StartDate).First());
			return this;
		}

		private ISkill withSkill(string name)
		{
			var existing = _skills.LoadAll().SingleOrDefault(x => x.Name == name);
			if (existing != null)
				return existing;

			var skillType = SkillTypeFactory.CreateSkillType();
			_skillTypes.Add(skillType);
			var staffingThresholds = new StaffingThresholds(new Percent(0.1), new Percent(0.2), new Percent(0.3));
			var midnightBreakOffset = new TimeSpan(3, 0, 0);
			var activity = new Activity(name);
			_activities.Add(activity);

			var skill = SkillFactory.CreateSkill(name, skillType, 15);
			skill.Activity = activity;
			skill.StaffingThresholds = staffingThresholds;
			skill.MidnightBreakOffset = midnightBreakOffset;
			_skills.Add(skill);

			return skill;
		}
		
		[UnitOfWork]
		public virtual Database WithActivity(string name)
		{
			_activity = name;
			_activities.Add(new Activity(name));
			return this;
		}

		public Database WithAssignment(string date)
		{
			WithAssignment(date.Date());
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
			_activity = activityName;
			startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
			endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);
			assignment().AddActivity(activity(), new DateTimePeriod(startTime, endTime));
			return this;
		}

		private IActivity activity()
		{
			if (_activity == null)
				WithActivity(RandomName.Make());
			return _activities.LoadAll().Single(x => x.Name.Equals(_activity));
		}

		private IPersonAssignment assignment()
		{
			var pa = _assignments.LoadAll().Single(x => x.Date == _date && x.Person == person());
			return _assignments.LoadAggregate(pa.Id.Value);
		}







		public virtual Database WithStateGroup(string name)
		{
			WithStateGroup(name, false);
			return this;
		}

		[UnitOfWork]
		public virtual Database WithStateGroup(string name, bool @default)
		{
			addStateGroup(name, @default);
			return this;
		}

		[UnitOfWork]
		public virtual Database WithStateCode(string code)
		{
			stateGroup().AddState(code, Guid.Empty);
			return this;
		}

		private IRtaStateGroup stateGroup()
		{
			if (_stateGroup == null)
				addStateGroup(RandomName.Make(), false);
			return _stateGroups.LoadAll().Single(x => x.Name == _stateGroup);
		}

		private void addStateGroup(string name, bool @default)
		{
			_stateGroup = name;
			var stateGroup = new RtaStateGroup(name, @default, true);
			_stateGroups.Add(stateGroup);
		}





		[UnitOfWork]
		public virtual Database WithRule(string name, int? staffingEffect, Adherence? adherence)
		{
			addRule(name, staffingEffect, adherence);
			return this;
		}

		private IRtaRule rule()
		{
			if (_rule == null)
				addRule(RandomName.Make(), null, null);
			return _rules.LoadAll().Single(x => x.Description.Name == _rule);
		}

		private void addRule(string name, int? staffingEffect, Adherence? adherence)
		{
			_rule = name;
			var rule = new RtaRule { Description = new Description(name) };
			if (staffingEffect.HasValue)
				rule.StaffingEffect = staffingEffect.Value;
			if (adherence.HasValue)
				rule.Adherence = adherence.Value;
			_rules.Add(rule);
		}






		[UnitOfWork]
		public virtual Database WithMapping()
		{
			var mapping = new RtaMap(stateGroup(), activity()) { RtaRule = rule() };
			_mappings.Add(mapping);
			return this;
		}

		[UnitOfWork]
		public virtual Database WithMapping(string stateGroupName, string ruleName)
		{
			_rule = ruleName;
			_stateGroup = stateGroupName;
			var mapping = new RtaMap(stateGroup(), null) { RtaRule = rule() };
			_mappings.Add(mapping);
			return this;
		}




		public Database PublishRecurringEvents()
		{
			_eventPublisher.Publish(new TenantMinuteTickEvent(), new TenantHourTickEvent());
			return this;
		}
		
	}
}