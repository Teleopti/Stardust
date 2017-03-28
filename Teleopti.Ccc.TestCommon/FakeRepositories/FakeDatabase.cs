using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public static class FakeDatabaseOrganizationExtensions
	{
		public static FakeDatabase WithTeam(this FakeDatabase database, Guid? id)
		{
			return database.WithTeam(id, null);
		}

		public static FakeDatabase WithTeam(this FakeDatabase database, string name)
		{
			return database.WithTeam(null, name);
		}

		public static FakeDatabase WithSite(this FakeDatabase database, Guid? id)
		{
			return database.WithSite(id, "s");
		}

		public static FakeDatabase WithSite(this FakeDatabase database, string name)
		{
			return database.WithSite(null, name);
		}

		public static FakeDatabase WithSite(this FakeDatabase database)
		{
			return database.WithSite(null, "s");
		}
	}

	public static class FakeDatabaseAgentExtensions
	{

		public static FakeDatabase WithAgent(this FakeDatabase database, string name)
		{
			return database.WithAgent(null, name, null, null, null, null, null, null);
		}
		
		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name)
		{
			return database.WithAgent(id, name, null, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid id)
		{
			return database.WithAgent(id, name, null, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? personId, string name, Guid? teamId, Guid? siteId)
		{
			return database.WithAgent(personId, name, null, teamId, siteId, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? personId, string name, Guid? teamId, Guid? siteId, Guid? businessUnitId)
		{
			return database.WithAgent(personId, name, null, teamId, siteId, businessUnitId, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			return database.WithAgent(personId, name, null, teamId, siteId, businessUnitId, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid? teamId)
		{
			return database.WithAgent(null, name, null, teamId, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid? teamId, Guid? siteId)
		{
			return database.WithAgent(null, name, null, teamId, siteId, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid? teamId, Guid? siteId, Guid? businessUnitId)
		{
			return database.WithAgent(null, name, null, teamId, siteId, businessUnitId, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate)
		{
			return database.WithAgent(null, name, terminalDate, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate, Guid? teamId)
		{
			return database.WithAgent(null, name, terminalDate, teamId, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate, Guid? teamId)
		{
			return database.WithAgent(id, name, terminalDate, teamId, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, int? employeeNumber)
		{
			return database.WithAgent(id, name, null, null, null, null, null, employeeNumber);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate)
		{
			return database.WithAgent(id, name, terminalDate, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, TimeZoneInfo timeZone)
		{
			return database.WithAgent(null, name, null, null, null, null, timeZone, null);
		}
		
		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate, TimeZoneInfo timeZone)
		{
			return database.WithAgent(null, name, terminalDate, null, null, null, timeZone, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, TimeZoneInfo timeZone)
		{
			return database.WithAgent(id, name, null, null, null, null, timeZone, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate, Guid? teamId, Guid? siteId, Guid? businessUnitId, TimeZoneInfo timeZone, int? employeeNumber)
		{
			database.WithPerson(id, name, terminalDate, timeZone, null, null, employeeNumber);
			database.WithPeriod(null, teamId, siteId, businessUnitId);
			database.WithExternalLogon(name);
			return database;
		}
	}

	public static class FakeDatabaseRuleExtensions
	{
		public static FakeDatabase WithMappedRule(this FakeDatabase database)
		{
			return database.WithMappedRule(null, "", null, 0, null, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode)
		{
			return database.WithMappedRule(null, stateCode, null, 0, stateCode, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, 0, null, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, int staffingEffect)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, staffingEffect, null, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, Guid? ruleId)
		{
			return database.WithMappedRule(ruleId, stateCode, activityId, 0, null, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, Guid? ruleId, string name)
		{
			return database.WithMappedRule(ruleId, stateCode, activityId, 0, name, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, string name)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, 0, name, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid activityId, int staffingEffect, Adherence adherence)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, staffingEffect, null, adherence);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, int staffingEffect, Adherence adherence)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, staffingEffect, null, adherence);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, Guid ruleId, string stateCode, Guid? activityId)
		{
			return database.WithMappedRule(ruleId, stateCode, activityId, 0, null, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, Guid ruleId, string stateCode, Guid? activityId, string name)
		{
			return database.WithMappedRule(ruleId, stateCode, activityId, 0, name, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, Guid? ruleId, string stateCode, Guid? activityId, int staffingEffect, string name, Adherence? adherence)
		{
			return database.WithMappedRule(ruleId, stateCode, activityId, staffingEffect, name, adherence, null);
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
		public static FakeDatabase WithPerson(this FakeDatabase database, Guid id, string name)
		{
			return database.WithPerson(id, name, null, null, null, null, null);
		}

		public static FakeDatabase WithPerson(this FakeDatabase database, string name)
		{
			return database.WithPerson(null, name, null, null, null, null, null);
		}
	}

	public static class FakeDatabaseScheduleExtensions
	{
		public static FakeDatabase WithScenario(this FakeDatabase database, Guid? id)
		{
			return database.WithScenario(id, null);
		}

		public static FakeDatabase WithActivity(this FakeDatabase database)
		{
			return database.WithActivity(null, null, null);
		}

		public static FakeDatabase WithActivity(this FakeDatabase database, Guid? id)
		{
			return database.WithActivity(id, null, null);
		}

		public static FakeDatabase WithDayOffTemplate(this FakeDatabase database, Guid? id)
		{
			return database.WithDayOffTemplate(id, null, null);
		}

		public static FakeDatabase WithDayOffTemplate(this FakeDatabase database, string name, string shortName)
		{
			return database.WithDayOffTemplate(null, name, shortName);
		}

		public static FakeDatabase WithAbsence(this FakeDatabase database, Guid? id)
		{
			return database.WithAbsence(id, null, null);
		}

		public static FakeDatabase WithAbsence(this FakeDatabase database, string name)
		{
			return database.WithAbsence(null, name, null);
		}
	}

	public class FakeDataSources
	{
		public List<KeyValuePair<string, int>> Datasources = new List<KeyValuePair<string, int>>();

		public void Add(string sourceId, int datasourceId)
		{
			Datasources.Add(new KeyValuePair<string, int>(sourceId, datasourceId));
		}
	}

	public class FakeDatabase
	{
		protected readonly FakeTenants _tenants;
		protected readonly FakePersonRepository _persons;
		protected readonly FakeBusinessUnitRepository _businessUnits;
		private readonly FakeSiteRepository _sites;
		private readonly FakeTeamRepository _teams;
		private readonly FakeContractRepository _contracts;
		private readonly FakePartTimePercentageRepository _partTimePercentages;
		private readonly FakeContractScheduleRepository _contractSchedules;
		private readonly FakeApplicationRoleRepository _applicationRoles;
		protected readonly FakeScenarioRepository _scenarios;
		private readonly FakeDayOffTemplateRepository _dayOffTemplates;
		protected readonly FakePersonAssignmentRepository _personAssignments;
		private readonly FakeApplicationFunctionRepository _applicationFunctions;
		private readonly FakeAvailableDataRepository _availableDatas;
		private readonly IDefinedRaptorApplicationFunctionFactory _allApplicationFunctions;
		private readonly FakeAbsenceRepository _absences;
		private readonly FakePersonAbsenceRepository _personAbsences;
		private readonly FakeActivityRepository _activities;
		private readonly FakeCommonAgentNameProvider _commonAgentNameProvider;
		private readonly FakeSkillRepository _skills;
		private readonly FakeGroupingReadOnlyRepository _groupings;
		protected readonly FakeRtaStateGroupRepository _stateGroups;
		protected readonly FakeRtaMapRepository _mappings;
		private readonly FakeRtaRuleRepository _rules;
		private readonly FakeExternalLogOnRepository _externalLogOns;
		private readonly FakeDataSources _dataSources;
		private readonly FakeSiteInAlarmReader _siteInAlarmReader;
		private readonly FakeTeamInAlarmReader _teamInAlarmReader;
		private readonly FakeMeetingRepository _meetings;


		private BusinessUnit _businessUnit;
		private Site _site;
		private Person _person;
		private PersonPeriod _personPeriod;
		private Team _team;
		private Contract _contract;
		private PartTimePercentage _partTimePercentage;
		private ContractSchedule _contractSchedule;
		private Scenario _scenario;
		private DayOffTemplate _dayOffTemplate;
		private PersonAssignment _personAssignment;
		private Absence _absence;
		private PersonAbsence _personAbsence;
		private Activity _activity;
		private Skill _skill;
		private RtaStateGroup _stateGroup;
		private RtaRule _rule;
		private Meeting _meeting;

		public static string DefaultTenantName = "default";
		public static Guid DefaultBusinessUnitId = Guid.NewGuid();

		public FakeDatabase(
			FakeTenants tenants,
			FakePersonRepository persons,
			FakeBusinessUnitRepository businessUnits,
			FakeSiteRepository sites,
			FakeTeamRepository teams,
			FakeContractRepository contracts,
			FakePartTimePercentageRepository partTimePercentages,
			FakeContractScheduleRepository contractSchedules,
			FakeApplicationRoleRepository applicationRoles,
			FakeScenarioRepository scenarios,
			FakeDayOffTemplateRepository dayOffTemplates,
			FakePersonAssignmentRepository personAssignments,
			FakeApplicationFunctionRepository applicationFunctions,
			FakeAvailableDataRepository availableDatas,
			IDefinedRaptorApplicationFunctionFactory allApplicationFunctions,
			FakeAbsenceRepository absences,
			FakePersonAbsenceRepository personAbsences,
			FakeActivityRepository activities,
			FakeCommonAgentNameProvider commonAgentNameProvider,
			FakeSkillRepository skills,
			FakeGroupingReadOnlyRepository groupings,
			FakeRtaStateGroupRepository stateGroups,
			FakeRtaMapRepository mappings,
			FakeRtaRuleRepository rules,
			FakeExternalLogOnRepository externalLogOns,
			FakeDataSources dataSources,
			FakeSiteInAlarmReader siteInAlarmReader,
			FakeTeamInAlarmReader teamInAlarmReader,
			FakeMeetingRepository meetings
			)
		{
			_tenants = tenants;
			_persons = persons;
			_businessUnits = businessUnits;
			_sites = sites;
			_teams = teams;
			_contracts = contracts;
			_partTimePercentages = partTimePercentages;
			_contractSchedules = contractSchedules;
			_applicationRoles = applicationRoles;
			_scenarios = scenarios;
			_dayOffTemplates = dayOffTemplates;
			_personAssignments = personAssignments;
			_applicationFunctions = applicationFunctions;
			_availableDatas = availableDatas;
			_allApplicationFunctions = allApplicationFunctions;
			_absences = absences;
			_personAbsences = personAbsences;
			_activities = activities;
			_commonAgentNameProvider = commonAgentNameProvider;
			_skills = skills;
			_groupings = groupings;
			_stateGroups = stateGroups;
			_externalLogOns = externalLogOns;
			_dataSources = dataSources;
			_siteInAlarmReader = siteInAlarmReader;
			_teamInAlarmReader = teamInAlarmReader;
			_mappings = mappings;
			_rules = rules;
			_meetings = meetings;

			createDefaultData();
		}
		
		private void createDefaultData()
		{
			// default data already created. ugly for now...
			if (_applicationFunctions.LoadAll().Count > 0)
				return;

			// all application functions
			_allApplicationFunctions.ApplicationFunctions.ForEach(_applicationFunctions.Add);

			// super role
			var role = new ApplicationRole { Name = SystemUser.SuperRoleName };
			role.SetId(SystemUser.SuperRoleId);
			role.AddApplicationFunction(_applicationFunctions.LoadAll().Single(x => x.FunctionPath == DefinedRaptorApplicationFunctionPaths.All));
			var availableData = new AvailableData
			{
				ApplicationRole = role,
				AvailableDataRange = AvailableDataRangeOption.Everyone
			};
			_availableDatas.Add(availableData);
			role.AvailableData = availableData;
			_applicationRoles.Add(role);

			// created by app config app
			// should should match the system user that is created
			// not sure if it does...
			WithPerson(SystemUser.Id, SystemUser.Name, null,
				TimeZoneInfo.Utc,
				CultureInfoFactory.CreateEnglishCulture(),
				CultureInfoFactory.CreateEnglishCulture(), null);
			_person.PermissionInformation.AddApplicationRole(role);

			WithTenant(DefaultTenantName, LegacyAuthenticationKey.TheKey);
			WithBusinessUnit(DefaultBusinessUnitId);

			WithScenario(null, true);

			// seems to always exist
			WithDataSource(-1, "-1");
			WithDataSource(1, "-1");
		}

		public Guid CurrentBusinessUnitId()
		{
			ensureExists(_businessUnits, null, () => WithBusinessUnit(null));
			return _businessUnit.Id.Value;
		}

		public Guid CurrentScenarioId()
		{
			ensureExists(_scenarios, null, () => WithScenario(null, true));
			return _scenario.Id.Value;
		}

		public Guid CurrentSiteId()
		{
			ensureExists(_sites, null, () => WithSite(null, null));
			return _site.Id.Value;
		}

		public Guid CurrentTeamId()
		{
			ensureExists(_teams, null, () => WithTeam(null, null));
			return _team.Id.Value;
		}
		
		public int CurrentDataSourceId()
		{
			return _dataSources.Datasources.Last().Value;
		}

		public FakeDatabase WithTenant(string tenant, string rtaKey)
		{
			_tenants.Has(tenant, rtaKey);
			return this;
		}

		public FakeDatabase WithTenant(string tenant)
		{
			_tenants.Has(tenant);
			return this;
		}

		public FakeDatabase WithDataSource(string sourceId)
		{
			var lastDataSourceId = _dataSources.Datasources.LastOrDefault().Value;
			return WithDataSource(lastDataSourceId + 1, sourceId);
		}

		public FakeDatabase WithDataSource(int datasourceId, string sourceId)
		{
			_dataSources.Add(sourceId, datasourceId);
			return this;
		}

		public FakeDatabase WithBusinessUnit(Guid? id)
		{
			_businessUnit = new BusinessUnit(RandomName.Make());
			_businessUnit.SetId(id ?? Guid.NewGuid());
			_businessUnits.Has(_businessUnit);
			return this;
		}

		public FakeDatabase WithSite(Guid? id, string name)
		{
			ensureExists(_businessUnits, null, () => WithBusinessUnit(null));
			_site = new Site(name ?? RandomName.Make());
			_site.SetBusinessUnit(_businessUnit);
			_site.SetId(id ?? Guid.NewGuid());
			_sites.Has(_site);
			return this;
		}

		public FakeDatabase WithTeam(Guid? id, string name)
		{
			ensureExists(_businessUnits, null, () => WithBusinessUnit(null));
			ensureExists(_sites, null, () => this.WithSite(null, null));
			_team = new Team {Site = _site};
			_team.SetId(id ?? Guid.NewGuid());
			if (name != null)
				_team.SetDescription(new Description(name));
			_teams.Has(_team);
			_site.AddTeam(_team);
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

		[UnitOfWork]
		public virtual FakeDatabase WithPerson(Guid? id, string name, string terminalDate, TimeZoneInfo timeZone, CultureInfo culture, CultureInfo uiCulture, int? employeeNumber)
		{
			var existing = _persons.LoadAll().FirstOrDefault(x => x.Id.GetValueOrDefault() == id.GetValueOrDefault());
			if (existing != null)
			{
				_person = existing as Person;
				return this;
			}

			Name setName;
			if (name.Contains(" "))
			{
				var fullName = name.Split(" ");
				setName = new Name(fullName[0], fullName[1]);
			}
			else setName = new Name(name, string.Empty);

			_person = new Person().WithName(setName);
			_person.SetId(id ?? Guid.NewGuid());
			_person.SetEmploymentNumber(employeeNumber?.ToString());
			_persons.Has(_person);
			
			_person.InTimeZone(timeZone ?? TimeZoneInfo.Utc);
			if (culture != null)
				_person.PermissionInformation.SetCulture(culture);
			if (uiCulture != null)
				_person.PermissionInformation.SetUICulture(uiCulture);

			if (terminalDate != null)
				WithTerminalDate(terminalDate);

			return this;
		}

		public FakeDatabase WithTerminalDate(string terminalDate)
		{
			_person.TerminatePerson(terminalDate.Date(), new PersonAccountUpdaterDummy());
			return this;
		}

		[UnitOfWork]
		public virtual FakeDatabase WithPeriod(string startDate, Guid? teamId, Guid? siteId, Guid? businessUnitId)
		{
			startDate = startDate ?? "2000-01-01";

			ensureExists(_businessUnits, businessUnitId, () => WithBusinessUnit(businessUnitId));
			ensureExists(_sites, siteId, () => this.WithSite(siteId));
			ensureExists(_teams, teamId, () => this.WithTeam(teamId));

			ensureExists(_contracts, null, () => WithContract(null));
			ensureExists(_partTimePercentages, null, () => WithPartTimePercentage(null));
			ensureExists(_contractSchedules, null, () => WithContractSchedule(null));

			var personContract = new PersonContract(_contract, _partTimePercentage, _contractSchedule);
			_person.AddPersonPeriod(new PersonPeriod(startDate.Date(), personContract, _team));
			_personPeriod = _person.Period(startDate.Date()) as PersonPeriod;

			return this;
		}

		[UnitOfWork]
		public virtual FakeDatabase WithExternalLogon(string name)
		{
			var exteralLogOn = new ExternalLogOn
			{
				DataSourceId = CurrentDataSourceId(),
				AcdLogOnOriginalId = name // this is what the rta receives
			};
			_externalLogOns.Add(exteralLogOn);
			_person.AddExternalLogOn(exteralLogOn, _personPeriod);
			return this;
		}

		public FakeDatabase WithAgentNameDisplayedAs(string format)
		{
			_commonAgentNameProvider.Has(new CommonNameDescriptionSetting {AliasFormat = format});
			return this;
		}

		public FakeDatabase WithRole(params string[] functionPaths)
		{
			return WithRole(AvailableDataRangeOption.Everyone, null, functionPaths);
		}

		public FakeDatabase WithRole(AvailableDataRangeOption availableDataRange, params string[] functionPaths)
		{
			return WithRole(availableDataRange, null, functionPaths);
		}

		public FakeDatabase WithRole(AvailableDataRangeOption availableDataRange, Guid? teamId, params string[] functionPaths)
		{
			var role = new ApplicationRole { Name = "role" };
			role.SetId(Guid.NewGuid());
			functionPaths.ForEach(p =>
			{
				role.AddApplicationFunction(_applicationFunctions.LoadAll().Single(x => x.FunctionPath == p));
			});
			var availableData = new AvailableData
			{
				ApplicationRole = role,
				AvailableDataRange = availableDataRange
			};

			if (teamId != null)
				availableData.AddAvailableTeam(_teams.Get(teamId.Value));

			_availableDatas.Add(availableData);
			role.AvailableData = availableData;
			_applicationRoles.Add(role);

			_person.PermissionInformation.AddApplicationRole(role);

			return this;
		}

		public FakeDatabase WithScenario(Guid? id, bool? @default)
		{
			ensureExists(_businessUnits, null, () => WithBusinessUnit(null));
			_scenario = new Scenario(RandomName.Make("scenario"));
			_scenario.SetId(id ?? Guid.NewGuid());
			if (@default.HasValue)
				_scenario.DefaultScenario = @default.Value;
			_scenario.SetBusinessUnit(_businessUnit);
			_scenarios.Has(_scenario);
			return this;
		}

		public FakeDatabase WithDayOffTemplate(Guid? id, string name, string shortName)
		{
			_dayOffTemplate = new DayOffTemplate(new Description(name ?? RandomName.Make(), shortName));
			_dayOffTemplate.SetId(id ?? Guid.NewGuid());
			_dayOffTemplates.Has(_dayOffTemplate);
			return this;
		}

		public FakeDatabase WithAssignment(Guid? personId, string date)
		{
			var existingPerson = _persons.LoadAll().SingleOrDefault(x => x.Id == personId);
			if (existingPerson != null)
				_person = existingPerson as Person;
			else
				WithPerson(personId, null, null, null, null, null, null);
			var existingAssignment = _personAssignments.LoadAll().SingleOrDefault(x => x.Person.Id == personId && x.Date == date.Date());
			if (existingAssignment != null)
				_personAssignment = existingAssignment as PersonAssignment;
			else
				WithAssignment(date);

			return this;
		}

		[UnitOfWork]
		public virtual FakeDatabase WithAssignment(string date)
		{
			ensureExists(_scenarios, null, () => WithScenario(null, true));
			_personAssignment = new PersonAssignment(_person, _scenario, date.Date());
			_personAssignment.SetId(Guid.NewGuid());
			_personAssignments.Has(_personAssignment);
			return this;
		}

		public FakeDatabase WithDayOff()
		{
			ensureExists(_dayOffTemplates, null, () => this.WithDayOffTemplate(null));
			_personAssignment.SetDayOff(_dayOffTemplate);
			return this;
		}

		public FakeDatabase WithAbsence(Guid? id, string name, bool? confidential)
		{
			_absence = new Absence();
			_absence.SetId(id ?? Guid.NewGuid());
			if (confidential.HasValue)
				_absence.Confidential = confidential.Value;
			_absence.Description = new Description(name ?? RandomName.Make());
			_absences.Has(_absence);
			return this;
		}

		public FakeDatabase WithConfidentialAbsence()
		{
			return WithAbsence(null, null, true);
		}

		public FakeDatabase WithPersonAbsence(string date)
		{
			// only correct for utc guys
			return WithPersonAbsence(date, date.Utc().AddHours(24).ToShortTimeString());
		}

		[UnitOfWork]
		public virtual FakeDatabase WithPersonAbsence(string start, string end)
		{
			ensureExists(_absences, null, () => this.WithAbsence(null, null, null));
			var period = new DateTimePeriod(start.Utc(), end.Utc());
			_personAbsence = new PersonAbsence(_person, _scenario, new AbsenceLayer(_absence, period));
			_personAbsence.SetId(Guid.NewGuid());
			_personAbsences.Has(_personAbsence);
			return this;
		}

		[UnitOfWork]
		public virtual FakeDatabase WithMeeting(string subject, string start, string end)
		{
			ensureExists(_scenarios, null, () => WithScenario(null, true));
			_meeting = new Meeting(
				_person, 
				new[] {new MeetingPerson(_person, false) }, 
				subject, 
				null, 
				null, 
				_activity, 
				_scenario)
			{
				StartDate = start.Date(),
				EndDate = end.Date(),
				StartTime = start.Utc().TimeOfDay,
				EndTime = end.Utc().TimeOfDay,
			};
			_meeting.SetId(Guid.NewGuid());
			_meetings.Has(_meeting);
			return this;
		}

		public FakeDatabase WithActivity(Guid? id, Color? color)
		{
			return WithActivity(id, RandomName.Make(), color);
		}

		public FakeDatabase WithActivity(Guid? id, string name)
		{
			return WithActivity(id, name, null);
		}

		public FakeDatabase WithActivity(Guid? id, string name, Color? color)
		{
			return WithActivity(id, name, null, color);
		}

		[UnitOfWork]
		public virtual FakeDatabase WithActivity(Guid? id, string name, string shortName, Color? color)
		{
			var existing = _activities.LoadAll().SingleOrDefault(x => x.Id == id);
			if (existing != null)
			{
				_activity = existing as Activity;
				return this;
			}

			name = name ?? RandomName.Make();
			_activity = new Activity {Description = new Description(name, shortName)};
			_activity.SetId(id ?? Guid.NewGuid());
			_activity.SetBusinessUnit(_businessUnit);
			if (color.HasValue)
				_activity.DisplayColor = color.Value;
			_activities.Has(_activity);

			return this;
		}

		[UnitOfWork]
		public virtual FakeDatabase WithAssignedActivity(string startTime, string endTime)
		{
			ensureExists(_activities, null, () => this.WithActivity(null));
			_personAssignment.AddActivity(_activity, new DateTimePeriod(startTime.Utc(), endTime.Utc()));
			return this;
		}

		public FakeDatabase WithSkill(Guid skillId)
		{
			ensureExists(_skills, skillId, () => withSkill(skillId));
			_person.AddSkill(_skill, _personPeriod);
			return this;
		}

		private void withSkill(Guid skillId)
		{
			var skill = new Skill();
			skill.SetId(skillId);
			_skills.Has(skill);
			_skill = skill;
		}
		
		public FakeDatabase WithStateGroup(Guid? id, string name)
		{
			return WithStateGroup(id, name, _stateGroups.LoadAll().IsEmpty());
		}

		public FakeDatabase WithStateGroup(Guid? id, string name, bool @default)
		{
			return WithStateGroup(id, name, @default, false);
		}

		[UnitOfWork]
		public virtual FakeDatabase WithStateGroup(Guid? id, string name, bool @default, bool isLoggedOut)
		{
			ensureExists(_businessUnits, null, () => WithBusinessUnit(null));
			_stateGroup = new RtaStateGroup(name ?? RandomName.Make(), @default, true);
			_stateGroup.IsLogOutState = isLoggedOut;
			_stateGroup.SetId(id ?? Guid.NewGuid());
			_stateGroup.SetBusinessUnit(_businessUnit);
			_stateGroups.Has(_stateGroup);
			return this;
		}

		[UnitOfWork]
		public virtual FakeDatabase WithStateCode(string stateCode)
		{
			ensureExists(_stateGroups, null, () => WithStateGroup(null, stateCode));
			_stateGroup.AddState(stateCode, stateCode);
			return this;
		}

		public FakeDatabase WithMappedRule(Guid? ruleId, string stateCode, Guid? activityId, int staffingEffect, string name, Adherence? adherence, Color? displayColor)
		{
			ensureExists(_businessUnits, null, () => WithBusinessUnit(null));

			_stateGroup = null;
			if (stateCode != null)
			{
				_stateGroup = (
					from g in _stateGroups.LoadAll()
					from s in g.StateCollection
					where s.StateCode == stateCode
					select g
					).FirstOrDefault() as RtaStateGroup;
				if (_stateGroup == null)
				{
					WithStateGroup(null, name);
					WithStateCode(stateCode);
				}
			}

			_activity = null;
			if (activityId != null)
			{
				ensureExists(_activities, activityId, () => WithActivity(activityId, null, null));
				_activity = _activities.LoadAll().Single(x => x.Id == activityId) as Activity;
			}

			_rule = null;
			if (ruleId != null)
				WithRule(ruleId, name, staffingEffect, adherence, displayColor);

			WithMapping();

			return this;
		}

		[UnitOfWork]
		public virtual FakeDatabase WithRule(Guid? id, string name, int staffingEffect, Adherence? adherence, Color? displayColor)
		{
			var existing = _rules.LoadAll().FirstOrDefault(x => x.Id.GetValueOrDefault() == id.GetValueOrDefault());
			if (existing != null)
			{
				_rule = existing as RtaRule;
				return this;
			}

			_rule = new RtaRule();
			if (name != null)
				_rule.Description = new Description(name);
			_rule.SetId(id ?? Guid.NewGuid());
			_rule.SetBusinessUnit(_businessUnit);
			_rule.StaffingEffect = staffingEffect;
			_rule.Adherence = adherence;
			_rule.DisplayColor = displayColor.GetValueOrDefault();
			_rules.Add(_rule);
			return this;
		}

		[UnitOfWork]
		public virtual FakeDatabase WithMapping()
		{
			var mapping = new RtaMap(_stateGroup, _activity) {RtaRule = _rule};
			mapping.SetId(Guid.NewGuid());
			mapping.SetBusinessUnit(_businessUnit);
			_mappings.Add(mapping);
			return this;
		}

		public FakeDatabase WithMapWithStateGroupWithoutStateCodes()
		{
			var stateGroup = new RtaStateGroup("Empty", false, true);
			stateGroup.SetId(Guid.NewGuid());
			stateGroup.SetBusinessUnit(_businessUnit);
			_stateGroups.Add(stateGroup);

			var mapping = new RtaMap(stateGroup, null) { RtaRule = _rule };
			mapping.SetId(Guid.NewGuid());
			mapping.SetBusinessUnit(_businessUnit);
			_mappings.Add(mapping);

			return this;
		}

		[UnitOfWork]
		public virtual FakeDatabase WithAlarm(TimeSpan threshold, Color? color)
		{
			_rule.IsAlarm = true;
			_rule.ThresholdTime = (int) threshold.TotalSeconds;
			_rule.AlarmColor = color.GetValueOrDefault();
			return this;
		}

		public FakeDatabase WithAlarm(TimeSpan threshold)
		{
			return WithAlarm(threshold, null);
		}

		[UnitOfWork]
		public virtual FakeDatabase ClearAssignments(Guid? personId)
		{
			var toRemove = _personAssignments.LoadAll().Where(x => x.Person.Id == personId).ToArray();
			toRemove.ForEach(_personAssignments.Remove);
			return this;
		}

		public FakeDatabase RemovePerson(Guid personId)
		{
			_persons.Remove(_persons.Get(personId));
			return this;
		}




		public FakeDatabase WithAgentState_DontUse(AgentStateReadModel agentStateReadModel)
		{
			_siteInAlarmReader.Has(agentStateReadModel);
			_teamInAlarmReader.Has(agentStateReadModel);
			return this;
		}
		
		public FakeDatabase OnSkill_DontUse(Guid skill)
		{
			_siteInAlarmReader.OnSkill(skill);
			_teamInAlarmReader.OnSkill(skill);
			return this;
		}

		public FakeDatabase InSkillGroupPage_DontUse()
		{
			_groupings
				.Has(new ReadOnlyGroupDetail
				{
					GroupId = _skill.Id.Value,
					PersonId = _person.Id.Value,
					SiteId = _site.Id.Value,
					TeamId = _team.Id.Value,
					FirstName = _person.Name.FirstName,
					LastName = _person.Name.LastName,
					EmploymentNumber = _person.EmploymentNumber
				});
			return this;
		}




		
		private static void ensureExists<T>(IRepository<T> loadAggregates, Guid? id, Action createAction)
			where T : IAggregateRoot
		{
			var all = loadAggregates.LoadAll();
			if (id.HasValue)
			{
				var existing = all.SingleOrDefault(x => x.Id.Equals(id));
				if (existing != null)
					return;
				createAction();
			}
			if (all.IsEmpty())
				createAction();
		}

	}
}