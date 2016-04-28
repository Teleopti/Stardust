using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
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
			database.WithPerson(id, name, terminalDate, timeZone, null, null);
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

		private BusinessUnit _businessUnit;
		private Site _site;
		private Person _person;
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
			FakeActivityRepository activities
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
			createDefaultData();
		}
		
		private void createDefaultData()
		{
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

		public FakeDatabase WithPerson(Guid? id, string name, string terminalDate, TimeZoneInfo timeZone, CultureInfo culture, CultureInfo uiCulture)
		{
			_person = new Person {Name = new Name(name, "")};
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
			ensureExists(_businessUnits, businessUnitId, () => WithBusinessUnit(businessUnitId));
			ensureExists(_sites, siteId, () => WithSite(siteId));
			ensureExists(_teams, teamId, () => WithTeam(teamId));

			ensureExists(_contracts, null, () => WithContract(null));
			ensureExists(_partTimePercentages, null, () => WithPartTimePercentage(null));
			ensureExists(_contractSchedules, null, () => WithContractSchedule(null));

			var personContract = new PersonContract(_contract, _partTimePercentage, _contractSchedule);
			_person.AddPersonPeriod(new PersonPeriod(startDate.Date(), personContract, _team));

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

		public FakeDatabase WithScenario(Guid? id)
		{
			_scenario = new Scenario(RandomName.Make("scenario"));
			_scenario.SetId(id ?? Guid.NewGuid());
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

		public FakeDatabase WithAssignment(string date, Guid person)
		{
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

		public FakeDatabase WithActivty2(Guid? id)
		{
			_activity = new Activity(RandomName.Make());
			_activity.SetId(id ?? Guid.NewGuid());
			_activities.Has(_activity);
			return this;
		}

		public FakeDatabase WithActivty(string startTime, string endTime)
		{
			ensureExists(_activities, null, () => this.WithActivty2(null));
			_personAssignment.AddActivity(_activity, new DateTimePeriod(startTime.Utc(), endTime.Utc()));
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