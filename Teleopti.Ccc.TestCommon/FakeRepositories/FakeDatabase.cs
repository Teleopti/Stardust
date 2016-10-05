using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
	}

	public static class FakeDatabaseAgentExtensions
	{

		public static FakeDatabase WithAgent(this FakeDatabase database, string name)
		{
			return database.WithAgent(null, name, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name)
		{
			return database.WithAgent(id, name, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? personId, string name, Guid? teamId, Guid? siteid)
		{
			return database.WithAgent(personId, name, null, teamId, siteid, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? personId, string name, Guid? teamId, Guid? siteid, Guid? businessUnitId)
		{
			return database.WithAgent(personId, name, null, teamId, siteid, businessUnitId, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid? teamId)
		{
			return database.WithAgent(null, name, null, teamId, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid? teamId, Guid? siteId)
		{
			return database.WithAgent(null, name, null, teamId, siteId, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid? teamId, Guid? siteId, Guid? businessUnitId)
		{
			return database.WithAgent(null, name, null, teamId, siteId, businessUnitId, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate)
		{
			return database.WithAgent(null, name, terminalDate, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate, Guid? teamId)
		{
			return database.WithAgent(null, name, terminalDate, teamId, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate, Guid? teamId)
		{
			return database.WithAgent(id, name, terminalDate, teamId, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate)
		{
			return database.WithAgent(id, name, terminalDate, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, TimeZoneInfo timeZone)
		{
			return database.WithAgent(null, name, null, null, null, null, timeZone);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate, TimeZoneInfo timeZone)
		{
			return database.WithAgent(null, name, terminalDate, null, null, null, timeZone);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, TimeZoneInfo timeZone)
		{
			return database.WithAgent(id, name, null, null, null, null, timeZone);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate, Guid? teamId, Guid? siteId, Guid? businessUnitId, TimeZoneInfo timeZone)
		{
			database.WithPerson(id, name, terminalDate, timeZone, null, null);
			database.WithPeriod(null, teamId, siteId, businessUnitId);
			database.WithExternalLogon(name);
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
		public static FakeDatabase WithPerson(this FakeDatabase database, Guid id, string name)
		{
			return database.WithPerson(id, name, null, null, null, null);
		}

		public static FakeDatabase WithPerson(this FakeDatabase database, string name)
		{
			return database.WithPerson(null, name, null, null, null, null);
		}
	}

	public static class FakeDatabaseScheduleExtensions
	{
		public static FakeDatabase WithScenario(this FakeDatabase database, Guid? id)
		{
			return database.WithScenario(id, null);
		}

		public static FakeDatabase WithActivity(this FakeDatabase database, Guid? id)
		{
			return database.WithActivity(id, null);
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
			return database.WithAbsence(id, null);
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
		private readonly FakeTenants _tenants;
		private readonly FakePersonRepository _persons;
		private readonly FakeBusinessUnitRepository _businessUnits;
		private readonly FakeSiteRepository _sites;
		private readonly FakeTeamRepository _teams;
		private readonly FakeContractRepository _contracts;
		private readonly FakePartTimePercentageRepository _partTimePercentages;
		private readonly FakeContractScheduleRepository _contractSchedules;
		private readonly FakeApplicationRoleRepository _applicationRoles;
		private readonly FakeScenarioRepository _scenarios;
		private readonly FakeDayOffTemplateRepository _dayOffTemplates;
		private readonly FakePersonAssignmentRepository _personAssignments;
		private readonly FakeApplicationFunctionRepository _applicationFunctions;
		private readonly FakeAvailableDataRepository _availableDatas;
		private readonly IDefinedRaptorApplicationFunctionFactory _allApplicationFunctions;
		private readonly FakeAbsenceRepository _absences;
		private readonly FakePersonAbsenceRepository _personAbsences;
		private readonly FakeActivityRepository _activities;
		private readonly FakeCommonAgentNameProvider _commonAgentNameProvider;
		private readonly FakeSkillRepository _skills;
		private readonly FakeGroupingReadOnlyRepository _groupings;
		private readonly FakeRtaStateGroupRepository _stateGroups;
		private readonly FakeRtaMapRepository _mappings;
		private readonly FakeExternalLogOnRepository _externalLogOns;
		private readonly FakeDataSources _dataSources;


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
		private Guid? _platform;
		private RtaStateGroup _stateGroup;
		private RtaRule _rtaRule;

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
			FakeExternalLogOnRepository externalLogOns,
			FakeDataSources dataSources
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
			_mappings = mappings;

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
				CultureInfoFactory.CreateEnglishCulture());
			_person.PermissionInformation.AddApplicationRole(role);

			WithTenant(DefaultTenantName, LegacyAuthenticationKey.TheKey);
			WithBusinessUnit(DefaultBusinessUnitId);

			// seems to always exist
			_dataSources.Add("-1", -1);
			_dataSources.Add("-1", 1);
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

		public Guid CurrentPlatform()
		{
			ensurePlatformExists(null);
			return _platform.Value;
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

		public FakeDatabase WithBusinessUnit(Guid? id)
		{
			_businessUnit = new BusinessUnit(RandomName.Make());
			_businessUnit.SetId(id ?? Guid.NewGuid());
			_businessUnits.Has(_businessUnit);
			return this;
		}

		public FakeDatabase WithSite(Guid? id, string name)
		{
			_site = new Site(name);
			_site.SetBusinessUnit(_businessUnit);
			_site.SetId(id ?? Guid.NewGuid());
			_sites.Has(_site);
			return this;
		}

		public FakeDatabase WithTeam(Guid? id, string name)
		{
			_team = new Team {Site = _site};
			_team.SetId(id ?? Guid.NewGuid());
			if (name != null)
				_team.Description = new Description(name);
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

		public FakeDatabase WithPerson(Guid? id, string name, string terminalDate, TimeZoneInfo timeZone, CultureInfo culture, CultureInfo uiCulture)
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

			_person = new Person {Name = setName};
			_person.SetId(id ?? Guid.NewGuid());
			_persons.Has(_person);

			if (timeZone != null)
				_person.PermissionInformation.SetDefaultTimeZone(timeZone);
			if (culture != null)
				_person.PermissionInformation.SetCulture(culture);
			if (uiCulture != null)
				_person.PermissionInformation.SetUICulture(uiCulture);

			if (terminalDate != null)
				_person.TerminatePerson(terminalDate.Date(), new PersonAccountUpdaterDummy());

			return this;
		}

		public FakeDatabase WithPeriod(string startDate, Guid? teamId, Guid? siteId, Guid? businessUnitId)
		{
			startDate = startDate ?? "2016-01-01";

			ensureExists(_businessUnits, businessUnitId, () => WithBusinessUnit(businessUnitId));
			ensureExists(_sites, siteId, () => this.WithSite(siteId));
			ensureExists(_teams, teamId, () => this.WithTeam(teamId));

			ensureExists(_contracts, null, () => WithContract(null));
			ensureExists(_partTimePercentages, null, () => WithPartTimePercentage(null));
			ensureExists(_contractSchedules, null, () => WithContractSchedule(null));

			var personContract = new PersonContract(_contract, _partTimePercentage, _contractSchedule);
			_personPeriod = new PersonPeriod(startDate.Date(), personContract, _team);
			_person.AddPersonPeriod(_personPeriod);

			return this;
		}

		public FakeDatabase WithDataSource(int datasourceId, string sourceId)
		{
			if (_dataSources.Datasources.Any(x => x.Key == sourceId))
				return this;
			_dataSources.Add(sourceId, datasourceId);
			return this;
		}

		public FakeDatabase WithExternalLogon(string name)
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
			var role = new ApplicationRole { Name = "role" };
			role.SetId(Guid.NewGuid());
			functionPaths.ForEach(p =>
			{
				role.AddApplicationFunction(_applicationFunctions.LoadAll().Single(x => x.FunctionPath == p));
			});
			var availableData = new AvailableData
			{
				ApplicationRole = role,
				AvailableDataRange = AvailableDataRangeOption.Everyone
			};
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

		public FakeDatabase WithAssignment(string date)
		{
			ensureExists(_scenarios, null, () => WithScenario(null, true));
			_personAssignment = new PersonAssignment(_person, _scenario, date.Date());
			_personAssignment.SetId(Guid.NewGuid());
			_personAssignments.Has(_personAssignment);
			return this;
		}
		
		public FakeDatabase WithAssignment(Guid? personId, string date)
		{
			var existingPerson = _persons.LoadAll().SingleOrDefault(x => x.Id == personId);
			if (existingPerson != null)
				_person = existingPerson as Person;
			else
				WithPerson(personId, null, null, null, null, null);
			var existingAssignment = _personAssignments.LoadAll().SingleOrDefault(x => x.Person.Id == personId && x.Date == date.Date());
			if (existingAssignment != null)
				_personAssignment = existingAssignment as PersonAssignment;
			else
				WithAssignment(date);

			return this;
		}

		public FakeDatabase WithDayOff()
		{
			ensureExists(_dayOffTemplates, null, () => this.WithDayOffTemplate(null));
			_personAssignment.SetDayOff(_dayOffTemplate);
			return this;
		}

		public FakeDatabase WithAbsence(Guid? id, bool? confidential)
		{
			_absence = new Absence();
			_absence.SetId(id ?? Guid.NewGuid());
			if (confidential.HasValue)
				_absence.Confidential = confidential.Value;
			_absences.Has(_absence);
			return this;
		}

		public FakeDatabase WithConfidentialAbsence()
		{
			return WithAbsence(null, true);
		}

		public FakeDatabase WithPersonAbsence(string date)
		{
			ensureExists(_absences, null, () => this.WithAbsence(null));
			_personAbsence = new PersonAbsence(_person, _scenario, new AbsenceLayer(_absence, new DateTimePeriod(date.Utc(), date.Utc().AddHours(24))));
			_personAbsence.SetId(Guid.NewGuid());
			_personAbsences.Has(_personAbsence);
			return this;
		}

		public FakeDatabase WithActivity(Guid? id, Color? color)
		{
			return WithActivity(id, RandomName.Make(), color);
		}

		public FakeDatabase WithActivity(Guid? id, string name, Color? color)
		{
			var existing = _activities.LoadAll().SingleOrDefault(x => x.Id == id);
			if (existing != null)
			{
				_activity = existing as Activity;
				return this;
			}

			_activity = new Activity(name ?? RandomName.Make());
			_activity.SetId(id ?? Guid.NewGuid());
			_activity.SetBusinessUnit(_businessUnit);
			if (color.HasValue)
				_activity.DisplayColor = color.Value;
			_activities.Has(_activity);

			return this;
		}

		public FakeDatabase WithAssignedActivity(string startTime, string endTime)
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

		public FakeDatabase InSkillGroupPage()
		{
			_groupings
				.Has(new ReadOnlyGroupDetail
				{
					GroupId = _skill.Id.Value,
					PersonId = _person.Id.Value,
					SiteId = _site.Id.Value,
					TeamId = _team.Id.Value,
					FirstName = _person.Name.FirstName,
					LastName = _person.Name.LastName
				});
			return this;
		}

		public FakeDatabase WithPlatform(Guid? platform)
		{
			_platform = platform ?? Guid.NewGuid();
			return this;
		}
		
		public FakeDatabase WithStateGroup(Guid? id, string name)
		{
			ensureExists(_businessUnits, null, () => WithBusinessUnit(null));
			_stateGroup = new RtaStateGroup(name ?? RandomName.Make());
			_stateGroup.SetId(id ?? Guid.NewGuid());
			_stateGroup.SetBusinessUnit(_businessUnit);
			_stateGroups.Has(_stateGroup);
			return this;
		}

		public FakeDatabase WithStateCode(string stateCode)
		{
			ensureExists(_stateGroups, null, () => this.WithStateGroup(null, null));
			ensurePlatformExists(null);
			_stateGroup.AddState(stateCode, stateCode, _platform.Value);
			return this;
		}

		public FakeDatabase WithRule(Guid? ruleId, string stateCode, Guid? platformTypeId, Guid? activityId, int staffingEffect, string name, Adherence? adherence, Color? displayColor)
		{
			ensurePlatformExists(platformTypeId);
			ensureExists(_businessUnits, null, () => WithBusinessUnit(null));

			_rtaRule = null;
			if (ruleId != null)
			{
				_rtaRule = new RtaRule();
				if (name != null)
					_rtaRule.Description = new Description(name);
				_rtaRule.SetId(ruleId);
				_rtaRule.SetBusinessUnit(_businessUnit);
				_rtaRule.StaffingEffect = staffingEffect;
				_rtaRule.Adherence = adherence;
				_rtaRule.DisplayColor = displayColor.GetValueOrDefault();
			}

			IRtaStateGroup stateGroup = null;
			if (stateCode != null)
			{
				stateGroup = (
					from g in _stateGroups.LoadAll()
					from s in g.StateCollection
					where s.StateCode == stateCode &&
						  s.PlatformTypeId == _platform
					select g
					).FirstOrDefault();
				if (stateGroup == null)
				{
					var isDefaultStateGroup = _stateGroups.LoadAll().IsEmpty();
					stateGroup = new RtaStateGroup(name, isDefaultStateGroup, true);
					stateGroup.SetId(Guid.NewGuid());
					stateGroup.SetBusinessUnit(_businessUnit);
					stateGroup.AddState(null, stateCode, _platform.Value);
					_stateGroups.Add(stateGroup);
				}
			}

			IActivity activity = null;
			if (activityId != null)
			{
				ensureExists(_activities, activityId, () => WithActivity(activityId, null));
				_activity = _activities.LoadAll().Single(x => x.Id == activityId) as Activity;
				activity = _activity;
			}

			var mapping = new RtaMap(stateGroup, activity) { RtaRule = _rtaRule };
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

			var mapping = new RtaMap(stateGroup, null) { RtaRule = _rtaRule };
			mapping.SetId(Guid.NewGuid());
			mapping.SetBusinessUnit(_businessUnit);
			_mappings.Add(mapping);

			return this;
		}

		public FakeDatabase WithAlarm(TimeSpan threshold, Color? color)
		{
			_rtaRule.IsAlarm = true;
			_rtaRule.ThresholdTime = (int) threshold.TotalSeconds;
			_rtaRule.AlarmColor = color.GetValueOrDefault();
			return this;
		}

		public FakeDatabase ClearScheduleData(Guid? personId)
		{
			var toRemove = _personAssignments.LoadAll().Where(x => x.Person.Id == personId).ToArray();
			toRemove.ForEach(_personAssignments.Remove);

			return this;
		}





		private void ensurePlatformExists(Guid? platform)
		{
			if (platform != null)
				WithPlatform(platform);
			if (_platform == null)
				WithPlatform(null);
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