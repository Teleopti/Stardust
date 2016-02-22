using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
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

		private BusinessUnit _businessUnit;
		private Site _site;
		private Person _person;
		private Team _team;
		private Contract _contract;
		private PartTimePercentage _partTimePercentage;
		private ContractSchedule _contractSchedule;

		public FakeDatabase(
			FakeTenants tenants,
			FakePersonRepository persons,
			FakeBusinessUnitRepository businessUnits,
			FakeSiteRepository sites,
			FakeTeamRepository teams,
			FakeContractRepository contracts,
			FakePartTimePercentageRepository partTimePercentages,
			FakeContractScheduleRepository contractSchedules,
			FakeApplicationRoleRepository applicationRoles
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
			createDefaultData();
		}

		private void createDefaultData()
		{
			// created by db scripts
			var superRole = new ApplicationRole { Name = "_Super Role" };
			superRole.SetId(SystemUser.SuperRoleId);
			_applicationRoles.Add(superRole);

			// created by app config app
			// should should match the system user that is created
			// not sure if it does...
			WithPerson(SystemUser.Id_AvoidUsing_This, "System", null,
				TimeZoneInfo.Utc,
				CultureInfoFactory.CreateEnglishCulture(),
				CultureInfoFactory.CreateEnglishCulture());
			//_person.PermissionInformation.AddApplicationRole(superRole);

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