using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Wfm.Adherence.Domain.Configuration;
using BusinessUnit = Teleopti.Ccc.Domain.Common.BusinessUnit;
using Person = Teleopti.Ccc.Domain.Common.Person;
using Team = Teleopti.Ccc.Domain.AgentInfo.Team;

namespace Teleopti.Ccc.TestCommon
{
	public class DatabaseLegacy
	{
		private readonly Database _database;
		private readonly AnalyticsDatabase _analyticsDatabase;
		private readonly IPersonRepository _persons;
		private readonly IContractRepository _contracts;
		private readonly IPartTimePercentageRepository _partTimePercentages;
		private readonly IContractScheduleRepository _contractSchedules;
		private readonly IExternalLogOnRepository _externalLogOns;
		private readonly ITeamRepository _teams;
		private readonly ISiteRepository _sites;
		private readonly IBusinessUnitRepository _businessUnits;

		public DatabaseLegacy(
			Database database,
			AnalyticsDatabase analyticsDatabase,
			IPersonRepository persons,
			IContractRepository contracts,
			IPartTimePercentageRepository partTimePercentages,
			IContractScheduleRepository contractSchedules,
			IExternalLogOnRepository externalLogOns,
			ITeamRepository teams,
			ISiteRepository sites,
			IBusinessUnitRepository businessUnits)
		{
			_database = database;
			_analyticsDatabase = analyticsDatabase;
			_persons = persons;
			_contracts = contracts;
			_partTimePercentages = partTimePercentages;
			_contractSchedules = contractSchedules;
			_externalLogOns = externalLogOns;
			_teams = teams;
			_sites = sites;
			_businessUnits = businessUnits;
		}

		public virtual DatabaseLegacy WithStateGroup(string name)
		{
			_database.WithStateGroup(name);
			_database.WithStateCode(name);
			return this;
		}
		
		[UnitOfWork]
		public virtual DatabaseLegacy WithAgent(string name)
		{
			var site = _sites.LoadAll().SingleOrDefault(x => x.Description.Name == "site");
			if (site == null)
			{
				site = new Site("site");
				_sites.Add(site);
			}
			var team = _teams.LoadAll().SingleOrDefault(x => x.Description.Name == "team");
			if (team == null)
			{
				team = new Team {Site = site}
					.WithDescription(new Description("team"));
				_teams.Add(team);
			}
			
			var businessUnit = _businessUnits.LoadAll().SingleOrDefault(x => x.Description.Name == "businessUnit");
			if (businessUnit == null)
			{
				businessUnit = new BusinessUnit("businessUnit");
				businessUnit.AddSite(site);
				_businessUnits.Add(businessUnit);
			}

			var contract = getOrAdd(_contracts, "contract", () =>
			{
				var c = new Contract("contract");
				_contracts.Add(c);
				return c;
			});

			var partTimePercentage = getOrAdd(_partTimePercentages, "partTimePercentage", () =>
			{
				var p = new PartTimePercentage("partTimePercentage");
				_partTimePercentages.Add(p);
				return p;
			});

			var contractSchedule = getOrAdd(_contractSchedules, "contractSchedule", () =>
			{
				var cs = new ContractSchedule("contractSchedule");
				_contractSchedules.Add(cs);
				return cs;
			});

			var personContract = new PersonContract(
				contract,
				partTimePercentage,
				contractSchedule);
			
			var personPeriod = new PersonPeriod("2000-01-01".Date(),
				personContract,
				team);

			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person.SetName(new Name(name, name));
			person.AddPersonPeriod(personPeriod);

			var exteralLogOn = new ExternalLogOn
			{
				AcdLogOnName = name, // is not used?
				DataSourceId = _analyticsDatabase.CurrentDataSourceId,	
				AcdLogOnOriginalId = name // this is what the rta receives
			};
			_externalLogOns.Add(exteralLogOn);
			person.AddExternalLogOn(exteralLogOn, personPeriod);

			_persons.Add(person);

			return this;
		}

		private static T getOrAdd<T>(IRepository<T> loadAggregates, string name, Func<T> createAction)
			where T : IAggregateRoot
		{
			var all = loadAggregates.LoadAll();
			var existing = all.SingleOrDefault(x => ((dynamic)x).Description.Name == name);
			if (existing == null)
				return createAction();
			return existing;
		}

		public DatabaseLegacy WithRule(string name, Adherence adherence)
		{
			_database.WithRule(name, null, adherence);
			return this;
		}

		public DatabaseLegacy WithMapping(string stateGroupName, string ruleName)
		{
			_database.WithMapping(stateGroupName, ruleName);
			return this;
		}

		public DatabaseLegacy WithDataSource(string sourceId)
		{
			_analyticsDatabase.WithDataSource(9, sourceId);
			return this;
		}

		public DatabaseLegacy PublishRecurringEvents()
		{
			_database.PublishRecurringEvents();
			return this;
		}
	}
}