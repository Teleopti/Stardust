using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

using Teleopti.Wfm.Adherence.Domain.Configuration;

namespace Teleopti.Ccc.TestCommon
{
	public class Database
	{
		private readonly AnalyticsDatabase _analytics;
		private readonly ICurrentEventPublisher _eventPublisher;
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
		private readonly IGroupingReadOnlyRepository _groupings;

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
			ICurrentEventPublisher eventPublisher,
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
			IExternalLogOnRepository externalLogOns,
			IGroupingReadOnlyRepository groupings
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
			_groupings = groupings;
		}

		[UnitOfWork]
		public virtual Guid CurrentPersonId()
		{
			return person().Id.Value;
		}

		[UnitOfWork]
		public virtual Guid CurrentStateGroupId()
		{
			return stateGroup().Id.Value;
		}

		[UnitOfWork]
		public virtual Guid CurrentScenarioId()
		{
			return scenario().Id.Value;
		}

		[UnitOfWork]
		public virtual IPersonAssignment CurrentAssignment()
		{
			return assignment(null);
		}

		[UnitOfWork]
		public virtual Guid CurrentSiteId()
		{
			return site(_site).Id.Value;
		}

		[UnitOfWork]
		public virtual Guid CurrentTeamId()
		{
			return team(_team).Id.Value;
		}

		[UnitOfWork]
		public virtual Guid TeamIdFor(string name)
		{
			return _teams.LoadAll().Where(x => x.Description.Name == name).Select(x => x.Id).Single().Value;
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


		public Database WithDefaultScenario(string name) => WithScenario(name, true);

		[UnitOfWork]
		public virtual Database WithScenario(string name, bool @default)
		{
			var scenario = new Scenario(name) {DefaultScenario = false, EnableReporting = true};
			_scenario = scenario.Description.Name;
			_scenarios.Add(scenario);
			return this;
		}

		private IScenario scenario()
		{
			if (_scenario != null)
				return _scenarios.LoadAll().Single(x => x.Description.Name == _scenario);
			_scenario = RandomName.Make();
			var s = new Scenario(_scenario) {DefaultScenario = true, EnableReporting = true};
			_scenarios.Add(s);
			return s;
		}


		[UnitOfWork]
		public virtual Database WithSite(string name)
		{
			site(name);
			return this;
		}

		[UnitOfWork]
		public virtual Database WithSite()
		{
			site(null);
			return this;
		}

		private ISite site(string name)
		{
			if (name != null)
			{
				var existing = _sites.LoadAll().SingleOrDefault(x => x.Description.Name == name);
				if (existing != null)
					return existing;
			}
			else
			{
				var existing = _sites.LoadAll().SingleOrDefault(x => x.Description.Name == _site);
				if (existing != null)
					return existing;
			}

			name = RandomName.Make();
			_site = name;
			var s = new Site(name);
			_sites.Add(s);
			return s;
		}

		[UnitOfWork]
		public virtual Database WithTeam(string name)
		{
			team(name);
			return this;
		}

		[UnitOfWork]
		public virtual Database WithTeam()
		{
			team(null);
			return this;
		}

		private ITeam team(string name)
		{
			name = name ?? RandomName.Make();
			var existing = _teams.LoadAll().SingleOrDefault(x => x.Description.Name == _team);
			if (existing != null)
			{
				_team = name;
				return existing;
			}

			_team = name;
			var s = site(_site);
			var t = new Team {Site = s}
				.WithDescription(new Description(_team));

			_teams.Add(t);
			s.AddTeam(t);
			return t;
		}


		public virtual Database WithPerson()
		{
			WithPerson(RandomName.Make());
			return this;
		}

		[UnitOfWork]
		public virtual Database WithPerson(string name)
		{
			var person = new Person().WithName(new Name(name, name));
			_person = person.Name.ToString();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			_persons.Add(person);
			return this;
		}

		[UnitOfWork]
		public virtual Database WithPersonPeriod(string date)
		{
			withPeriod(person(), date.Date());
			return this;
		}

		[UnitOfWork]
		public virtual Database WithAgent()
		{
			return withAgent(RandomName.Make());
		}

		[UnitOfWork]
		public virtual Database WithAgent(string name)
		{
			return withAgent(name);
		}

		[UnitOfWork]
		public virtual Database WithAgent(string name, string employmentNumber)
		{
			return withAgent(name, employmentNumber);
		}

		[UnitOfWork]
		public virtual Database WithTerminatedAgent(string date)
		{
			return withAgent(RandomName.Make(), null, date.Date());
		}

		private Database withAgent(string name, string employmentNumber = null, DateOnly? terminationDate = null)
		{
			var existing = _persons.LoadAll().SingleOrDefault(x => x.Name.FirstName == name);
			if (existing != null)
			{
				_person = new Name(name, name).ToString();
				return this;
			}

			var person = new Person().WithName(new Name(name, name));
			_person = person.Name.ToString();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person.SetEmploymentNumber(employmentNumber ?? string.Empty);

			var personPeriod = withPeriod(person, "2001-01-01".Date());
			if (terminationDate != null)
				person.TerminatePerson(terminationDate.Value, new PersonAccountUpdaterDummy());

			withExternalLogon(name, person, personPeriod);

			_persons.Add(person);

			return this;
		}

		private PersonPeriod withPeriod(IPerson person, DateOnly date)
		{
			var personContract = new PersonContract(contract(), partTimePercentage(), contractSchedule());
			var personPeriod = new PersonPeriod(date, personContract, team(_team));
			person.AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		[UnitOfWork]
		public virtual Database WithExternalLogon(string name)
		{
			var person = this.person();
			return withExternalLogon(name, person, person.PersonPeriodCollection.Last());
		}

		private Database withExternalLogon(string name, IPerson person, IPersonPeriod personPeriod)
		{
			var exteralLogOn = new ExternalLogOn
			{
				//AcdLogOnName = name, // is not used?
				AcdLogOnMartId = -1, // NotDefined should be there, 0 probably wont
				DataSourceId = _analytics.CurrentDataSourceId,
				AcdLogOnOriginalId = name // this is what the rta receives
			};
			_externalLogOns.Add(exteralLogOn);
			person.AddExternalLogOn(exteralLogOn, personPeriod);
			return this;
		}

		private IPerson person()
		{
			return _persons.LoadAll().Single(x => x.Name.ToString() == _person);
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
		public virtual Database WithSkill(string name)
		{
			var skill = withSkill(name);

			var personSkill = new PersonSkill(skill, new Percent(100));
			person().AddSkill(personSkill, person().PersonPeriodCollection.OrderByDescending(x => x.StartDate).First());
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

		public virtual Database WithActivity(string name) => WithActivity(name, null);

		[UnitOfWork]
		public virtual Database WithActivity(string name, Color? color)
		{
			activity(name, color);
			return this;
		}

		[UnitOfWork]
		public virtual Database WithAssignment(string date)
		{
			withAssignment(date);
			return this;
		}

		private IPersonAssignment withAssignment(string date)
		{
			_date = date.Date();
			var personAssignment = new PersonAssignment(person(), scenario(), _date);
			_assignments.Add(personAssignment);
			return personAssignment;
		}

		[UnitOfWork]
		public virtual Database WithAssignedActivity(string activityName, string startTime, string endTime)
		{
			assignment(startTime)
				.AddActivity(activity(activityName, null), new DateTimePeriod(startTime.Utc(), endTime.Utc()));
			return this;
		}

		private IActivity activity(string name, Color? color)
		{
			if (name != null)
				_activity = name;

			var existing = _activities.LoadAll().SingleOrDefault(x => x.Name == _activity);
			if (existing != null)
				return existing;

			_activity = RandomName.Make();
			var activity = new Activity(_activity);
			if (color.HasValue)
				activity.DisplayColor = color.Value;
			_activities.Add(activity);
			return activity;
		}

		private IPersonAssignment assignment(string date)
		{
			if (date != null)
				_date = date.Date();

			var existing = _assignments.LoadAll().SingleOrDefault(x => x.Date == _date && x.Person == person());
			if (existing != null)
				return existing;

			var personAssignment = new PersonAssignment(person(), scenario(), _date);
			_assignments.Add(personAssignment);
			return personAssignment;
		}


		public virtual Database WithStateGroup(string name)
		{
			WithStateGroup(name, false);
			return this;
		}

		public Database WithStateGroup(string name, bool @default)
		{
			return WithStateGroup(name, @default, false);
		}

		[UnitOfWork]
		public virtual Database WithStateGroup(string name, bool @default, bool isLogOutState)
		{
			addStateGroup(name, @default, isLogOutState);
			return this;
		}

		[UnitOfWork]
		public virtual Database WithStateCode(string code)
		{
			stateGroup().AddState(code);
			return this;
		}

		private IRtaStateGroup stateGroup()
		{
			if (_stateGroup == null)
				addStateGroup(RandomName.Make(), false, false);
			return _stateGroups.LoadAll().Single(x => x.Name == _stateGroup);
		}

		private void addStateGroup(string name, bool @default, bool isLogOutState)
		{
			_stateGroup = name;
			var stateGroup = new RtaStateGroup(name, @default, true)
			{
				IsLogOutState = isLogOutState
			};
			_stateGroups.Add(stateGroup);
		}


		public virtual Database WithRule(string name, int? staffingEffect, Adherence? adherence) =>
			WithRule(name, staffingEffect, adherence, null);

		[UnitOfWork]
		public virtual Database WithRule(string name, int? staffingEffect, Adherence? adherence, Color? color) =>
			addRule(name, staffingEffect, adherence, color);

		private IRtaRule rule()
		{
			if (_rule == null)
				addRule(RandomName.Make(), null, null, null);
			return _rules.LoadAll().Single(x => x.Description.Name == _rule);
		}

		private Database addRule(string name, int? staffingEffect, Adherence? adherence, Color? color)
		{
			_rule = name;
			var rule = new RtaRule {Description = new Description(name)};
			if (staffingEffect.HasValue)
				rule.StaffingEffect = staffingEffect.Value;
			if (adherence.HasValue)
				rule.Adherence = adherence.Value;
			if (color.HasValue)
				rule.DisplayColor = color.Value;
			_rules.Add(rule);
			return this;
		}


		[UnitOfWork]
		public virtual Database WithMapping()
		{
			var mapping = new RtaMap(stateGroup(), activity(null, null)) {RtaRule = rule()};
			_mappings.Add(mapping);
			return this;
		}

		[UnitOfWork]
		public virtual Database WithMapping(string stateGroupName, string ruleName)
		{
			_rule = ruleName;
			_stateGroup = stateGroupName;
			var mapping = new RtaMap(stateGroup(), null) {RtaRule = rule()};
			_mappings.Add(mapping);
			return this;
		}

		public Database PublishRecurringEvents()
		{
			_eventPublisher.Current().Publish(new TenantMinuteTickEvent(), new TenantHourTickEvent());
			return this;
		}

		[UnitOfWork]
		public virtual Database UpdateGroupings()
		{
			_groupings.UpdateGroupingReadModel(_persons.LoadAll().Select(x => x.Id.Value).ToArray());
			return this;
		}
	}
}